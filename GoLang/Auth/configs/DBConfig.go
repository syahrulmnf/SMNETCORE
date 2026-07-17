package configs

import (
	"auth-go/extensions"
	"auth-go/logger"
	"sort"
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

	if len(DBConfigsData) == 0 {
		LoadDBConfigs()
	}
	dbConfig, ok := DBConfigsData["Database"]
	if ok && dbConfig != nil {
		return dbConfig, nil
	}

	if !ok || dbConfig == nil {
		logger.Logger.Error("Database config invalid structure")
		panic("Database config invalid")
	}
	panic("Database config invalid")
}

func (t *DBConfigs) Get(tenant string) (bool, error) {
	dbConfig, _ := GetDBConfigs(tenant)
	t = dbConfig
	return true, nil
}

var DBConfigsData map[string]*DBConfigs

func LoadDBConfigs() {
	DBConfigsData = GetDatabasesList()
}

// configuredDatabases converts the Database configuration section into a
// stable list so all database checks can safely run in parallel.
func GetDatabasesList() map[string]*DBConfigs {
	databaseValue, exists := AppConfig.Values["Database"]
	if !exists || databaseValue == nil {
		return nil
	}

	databaseMap, ok := databaseValue.(map[string]any)
	if !ok {
		return nil
	}

	names := make([]string, 0, len(databaseMap))
	for name := range databaseMap {
		names = append(names, name)
	}
	sort.Strings(names)

	result := make(map[string]*DBConfigs)
	for _, name := range names {
		if config := databaseConfig(databaseMap[name]); config != nil {
			result[name] = config
		}
	}
	return result
}

func databaseConfig(value any) *DBConfigs {
	if config, ok := value.(*DBConfigs); ok && config != nil {
		copy := *config
		return &copy
	}
	if config, ok := value.(DBConfigs); ok {
		return &config
	}

	values, ok := value.(map[string]any)
	if !ok {
		return nil
	}

	return &DBConfigs{
		Drivers:  extensions.MapValueToString(values, "drivers"),
		Host:     extensions.MapValueToString(values, "db_host"),
		Port:     extensions.MapValueToString(values, "db_port"),
		User:     extensions.MapValueToString(values, "db_user"),
		Password: extensions.MapValueToString(values, "db_password"),
		DBName:   extensions.MapValueToString(values, "db_name"),
	}
}
