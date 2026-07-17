package repositories

import (
	"auth-go/common"
	"auth-go/configs"
	authgorm "auth-go/gorm"
	"errors"
	"sync"

	"gorm.io/gorm"
)

type GenericRepo struct {
	Driver *DBDriver
}

// NewTenantRepo creates a repository that uses an already configured DBDriver.
func NewGenericRepo(driver *DBDriver) *GenericRepo {
	return &GenericRepo{Driver: driver}
}

// CreateByTenant configures the repository using the database configured for tenant.
func (t *GenericRepo) CreateByTenant() error {
	tenant := common.GlobalsData.GenericDB
	if t == nil {
		t = &GenericRepo{}
	}

	dbDriver := &DBDriver{}
	if err := dbDriver.CreateByTenant(tenant); err != nil {
		return err
	}

	t.Driver = dbDriver
	return nil
}

// Create configures the repository with the supplied database configuration.
func (t *GenericRepo) Create(db *configs.DBConfigs) error {
	dbDriver := &DBDriver{}
	if err := dbDriver.Create(db); err != nil {
		return err
	}

	t.Driver = dbDriver
	return nil
}

func (u *GenericRepo) db() (*gorm.DB, error) {
	if u == nil || u.Driver == nil || u.Driver.DB == nil {
		return nil, ErrUserRepositoryNotInitialized
	}
	return u.Driver.DB, nil
}

// GetGenericTenant resolves the tenant for username. It first uses the most
// recently modified generic user-to-tenant mapping. When no mapping exists,
// it checks every configured database concurrently and returns the first
// configured database that contains both the user and a tenant record.
func (u *GenericRepo) GetGenericTenant(username string) (string, error) {
	db, err := u.db()
	if err != nil {
		return "", err
	}

	var userTenant authgorm.UerTenant
	err = db.Where("user_name = ?", username).Order("modified DESC").First(&userTenant).Error
	if err == nil {
		return userTenant.Tenant, nil
	}
	if !errors.Is(err, gorm.ErrRecordNotFound) {
		return "", err
	}

	databaseConfigs := configs.DBConfigsData
	if len(databaseConfigs) == 0 {
		return "", ErrGenericTenantInvalidCredentials
	}

	type tenantResult struct {
		index  string
		tenant string
	}
	results := make(chan tenantResult, len(databaseConfigs))
	var waitGroup sync.WaitGroup
	for index, databaseConfig := range databaseConfigs {
		waitGroup.Add(1)
		go func(index string, databaseConfig *configs.DBConfigs) {
			defer waitGroup.Done()

			driver := &DBDriver{}
			if err := driver.Create(databaseConfig); err != nil {
				return
			}
			defer driver.CloseConnection()

			var user authgorm.UserLoginTable
			if err := driver.DB.Where("email_address = ? OR display_name = ?", username, username).First(&user).Error; err != nil {
				return
			}

			var tenant authgorm.TenantTable
			if err := driver.DB.First(&tenant).Error; err != nil {
				return
			}
			results <- tenantResult{index: index, tenant: tenant.Name}
		}(index, databaseConfig)
	}

	waitGroup.Wait()
	close(results)

	for result := range results {
		if result.tenant != "" {
			return result.tenant, nil
		}
	}

	return "", ErrGenericTenantInvalidCredentials
}
