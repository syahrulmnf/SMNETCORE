package repositories

import (
	"auth-go/configs"
)

type TenantRepo struct {
	Tenant   string
	tenantDB string
	// Add any necessary fields or dependencies here
}

func (t *TenantRepo) GetTenant() string {
	dbConfig, ok := configs.AppConfig.Values["Database"]
	if ok && dbConfig != nil {
		tenantConfig, ok := dbConfig.(map[string]any)[t.Tenant].(map[string]any)
		if ok && tenantConfig != nil {

		}
	}
	return t.Tenant
}
