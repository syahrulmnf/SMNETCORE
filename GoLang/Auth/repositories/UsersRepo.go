package repositories

import (
	"auth-go/configs"
	"auth-go/extensions"
	authgorm "auth-go/gorm"
	"crypto/sha256"
	"crypto/subtle"
	"encoding/hex"
	"errors"
	"strings"
	"time"

	"golang.org/x/crypto/bcrypt"
	"gorm.io/gorm"
)

// UsersRepo provides credential and tenant-membership queries for users.
type UsersRepo struct {
	Driver *DBDriver
}

func NewUsersRepo(driver *DBDriver) *UsersRepo {
	return &UsersRepo{Driver: driver}
}

// CreateByTenant configures the repository using the database for tenant.
func (u *UsersRepo) CreateByTenant(tenant string) error {
	if u == nil {
		return ErrUserRepositoryNotInitialized
	}
	driver := &DBDriver{}
	if err := driver.CreateByTenant(tenant); err != nil {
		return err
	}
	u.Driver = driver
	return nil
}

// Create configures the repository with an explicit database configuration.
func (u *UsersRepo) Create(db *configs.DBConfigs) error {
	if u == nil {
		return ErrUserRepositoryNotInitialized
	}
	driver := &DBDriver{}
	if err := driver.Create(db); err != nil {
		return err
	}
	u.Driver = driver
	return nil
}

func (u *UsersRepo) db() (*gorm.DB, error) {
	if u == nil || u.Driver == nil || u.Driver.DB == nil {
		return nil, ErrUserRepositoryNotInitialized
	}
	return u.Driver.DB, nil
}

// Get finds an enabled user by email address or display name.
func (u *UsersRepo) Get(username string) (authgorm.UserLoginTable, error) {
	db, err := u.db()
	if err != nil {
		return authgorm.UserLoginTable{}, err
	}

	var user authgorm.UserLoginTable
	err = db.Where("(email_address = ? OR display_name = ?) AND deleted = ? AND allow_login = ?", username, username, false, true).
		First(&user).Error
	if err != nil {
		return authgorm.UserLoginTable{}, err
	}
	return user, nil
}

// GetUserTenants returns active tenant-login assignments for a user.
func (u *UsersRepo) GetUserTenants(userID int64) ([]authgorm.UserTenantLoginTable, error) {
	db, err := u.db()
	if err != nil {
		return nil, err
	}
	var memberships []authgorm.UserTenantLoginTable
	res := db.Where("user_id = ? AND deleted = ? AND active = ? AND allow_login = ?", userID, false, true, true).
		Find(&memberships)
	if res.Error != nil {
		return nil, res.Error
	}
	return memberships, nil
}

// GetUsersTenant is kept as a descriptive alias for callers using the original naming.
func (u *UsersRepo) GetUsersTenant(userID int64) ([]authgorm.UserTenantLoginTable, error) {
	return u.GetUserTenants(userID)
}

// GetRoles returns the comma-separated roles assigned to a user for tenant.
func (u *UsersRepo) GetRoles(userID int64, tenant string) ([]string, error) {
	db, err := u.db()
	if err != nil {
		return nil, err
	}

	var userRoles authgorm.UserRoles
	err = db.Model(&authgorm.UserRoles{}).
		Joins("JOIN tenant_tables ON tenant_tables.id = user_roles.tenant_id").
		Where("user_roles.user_id = ? AND tenant_tables.name = ?", userID, tenant).
		First(&userRoles).Error
	if errors.Is(err, gorm.ErrRecordNotFound) {
		return []string{}, nil
	}
	if err != nil {
		return nil, err
	}
	return extensions.SplitString(userRoles.Roles, ","), nil
}

// SaveLoginCred persists an issued JWT and the claims used to create it.
func (u *UsersRepo) SaveLoginCred(loginCred *authgorm.LoginCred) error {
	db, err := u.db()
	if err != nil {
		return err
	}
	return db.Create(loginCred).Error
}

// ExpireLoginCred marks the active credential for a token as logged out.
func (u *UsersRepo) ExpireLoginCred(token, tenant string, userID int64) error {
	db, err := u.db()
	if err != nil {
		return err
	}

	result := db.Model(&authgorm.LoginCred{}).
		Where("tokens = ? AND tenant = ? AND user_id = ? AND active = ?", token, tenant, userID, true).
		Updates(map[string]any{
			"expired":    time.Now(),
			"active":     false,
			"is_expired": true,
		})
	if result.Error != nil {
		return result.Error
	}
	if result.RowsAffected == 0 {
		return gorm.ErrRecordNotFound
	}
	return nil
}

// GetLoginCred marks the active credential for a token as logged out.
func (u *UsersRepo) GetLoginCred(token, tenant string, userID int64) (*authgorm.LoginCred, error) {
	db, err := u.db()
	if err != nil {
		return nil, err
	}

	var results authgorm.LoginCred
	rst := db.Where("tokens = ? AND tenant = ? AND user_id = ? AND active = ?", token, tenant, userID, true).First(&results)
	if rst.Error != nil {
		return nil, rst.Error
	}
	if &results == nil {
		return nil, gorm.ErrRecordNotFound
	}
	return &results, nil
}

// VerifyLogin validates the user's password and confirms access to tenant.
func (u *UsersRepo) VerifyLogin(username, password, tenant string) (authgorm.UserLoginTable, error) {
	user, err := u.Get(username)
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return authgorm.UserLoginTable{}, ErrInvalidCredentials
		}
		return authgorm.UserLoginTable{}, err
	}
	if !verifyPassword(user.Password, user.Salt, password) {
		return authgorm.UserLoginTable{}, ErrInvalidCredentials
	}

	db, err := u.db()
	if err != nil {
		return authgorm.UserLoginTable{}, err
	}
	var membership authgorm.UserTenantLoginTable
	err = db.Joins("JOIN tenant_tables ON tenant_tables.id = user_tenant_login_tables.tenant_id").
		Where("user_tenant_login_tables.user_id = ? AND tenant_tables.name = ? AND user_tenant_login_tables.deleted = ? AND user_tenant_login_tables.active = ? AND user_tenant_login_tables.allow_login = ?", user.Id, tenant, false, true, true).
		First(&membership).Error
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return authgorm.UserLoginTable{}, ErrUserNotAllowedForTenant
		}
		return authgorm.UserLoginTable{}, err
	}
	return user, nil
}

func verifyPassword(encodedPassword, salt, password string) bool {
	if encodedPassword == "" {
		return false
	}
	if strings.HasPrefix(encodedPassword, "$2") {
		return bcrypt.CompareHashAndPassword([]byte(encodedPassword), []byte(password)) == nil
	}

	// Support existing SHA-256-with-salt records while password hashes are migrated
	// to bcrypt. Both common salt concatenation orders are checked in constant time.
	passwordThenSalt := sha256.Sum256([]byte(password + salt))
	saltThenPassword := sha256.Sum256([]byte(salt + password))
	if subtle.ConstantTimeCompare([]byte(strings.ToLower(encodedPassword)), []byte(hex.EncodeToString(passwordThenSalt[:]))) == 1 ||
		subtle.ConstantTimeCompare([]byte(strings.ToLower(encodedPassword)), []byte(hex.EncodeToString(saltThenPassword[:]))) == 1 {
		return true
	}
	return subtle.ConstantTimeCompare([]byte(encodedPassword), []byte(password)) == 1
}
