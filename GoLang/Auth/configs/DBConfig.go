package configs

import (
	"auth-go/logger"
)

var DBDriversType []string = []string{
	"mysql",
	"postgres",
	"sqlite",
	"mssql",
	"oracle",
}

type DBConfigs struct {
	Drivers  string `json:"drivers"`
	Host     string `json:"db_host"`
	Port     string `json:"db_port"`
	User     string `json:"db_user"`
	Password string `json:"db_password"`
	DBName   string `json:"db_name"`
}

func GetDBConfigs(tenant string) (*DBConfigs, error) {
	dbc := &DBConfigs{}
	dbConfig, ok := AppConfig.Values["Database"]
	if ok && dbConfig != nil {
		// dbConfig is expected to be map[string]any -> tenant -> map[string]any
		dbMap, ok := dbConfig.(map[string]any)
		if !ok || dbMap == nil {
			logger.Logger.Error("Database config invalid structure")
			panic("Database config invalid")
		}

		var tenantMap map[string]any
		if v, exists := dbMap[tenant]; exists && v != nil {
			if m, ok := v.(map[string]any); ok {
				tenantMap = m
			}
		}
		if tenantMap == nil {
			if v, exists := dbMap["Generic"]; exists && v != nil {
				if m, ok := v.(map[string]any); ok {
					tenantMap = m
				}
			}
		}
		if tenantMap == nil {
			logger.Logger.Error("Database config was not found", "Tenant: ", tenant)
			panic("Data was not found")
		}

		// helper to read string values
		getStr := func(key string) string {
			if val, ok := tenantMap[key]; ok && val != nil {
				if s, ok := val.(string); ok {
					return s
				}
			}
			return ""
		}
		dbc = &DBConfigs{
			Drivers:  getStr("drivers"),
			Host:     getStr("db_host"),
			Port:     getStr("db_port"),
			User:     getStr("db_user"),
			Password: getStr("db_password"),
			DBName:   getStr("db_name"),
		}
	}
	return dbc, nil
}

func (t *DBConfigs) Get(tenant string) (bool, error) {
	dbConfig, _ := GetDBConfigs(tenant)
	t = dbConfig
	return true, nil
}
