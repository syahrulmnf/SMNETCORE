package repositories

import (
	"auth-go/configs"
	"auth-go/gorm"
	"errors"
)

type TenantRepo struct {
	Tenant   string
	tenantDB string
	Secret   string
	Host     string
	Driver   *DBDriver
	// Add any necessary fields or dependencies here
}

// NewTenantRepo creates a repository that uses an already configured DBDriver.
func NewTenantRepo(driver *DBDriver) *TenantRepo {
	return &TenantRepo{Driver: driver}
}

// CreateByTenant configures the repository using the database configured for tenant.
func (t *TenantRepo) CreateByTenant(tenant string) error {
	if t == nil {
		t = &TenantRepo{}
	}
	dbDriver := &DBDriver{}
	if err := dbDriver.CreateByTenant(tenant); err != nil {
		return err
	}

	t.Driver = dbDriver
	return nil
}

// Create configures the repository with the supplied database configuration.
func (t *TenantRepo) Create(db *configs.DBConfigs) error {
	dbDriver := &DBDriver{}
	if err := dbDriver.Create(db); err != nil {
		return err
	}

	t.Driver = dbDriver
	return nil
}

// GetTenant returns the tenant record whose Name matches tenant.
func (t *TenantRepo) GetTenant(tenant string) (gorm.TenantTable, error) {
	if t == nil || t.Driver == nil || t.Driver.DB == nil {
		return gorm.TenantTable{}, errors.New("tenant repository is not initialized")
	}

	var tenantTable gorm.TenantTable
	if err := t.Driver.DB.Where("name = ?", tenant).First(&tenantTable).Error; err != nil {
		return gorm.TenantTable{}, err
	}

	t.Tenant = tenantTable.Name
	t.Secret = tenantTable.Secret
	return tenantTable, nil
}

// GetSecret returns the configured secret for tenant, or an empty string when it is not found.
func (t *TenantRepo) GetSecret(tenant string) string {
	if _, err := t.GetTenant(tenant); err != nil {
		return ""
	}

	return t.Secret
}
