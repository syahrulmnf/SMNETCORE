package repositories

import (
	"auth-go/configs"
	drivers "auth-go/repositories/drivers"
	"context"
	"errors"
	"log"
	"time"

	"gorm.io/driver/mysql"
	"gorm.io/driver/postgres"
	"gorm.io/driver/sqlite"
	"gorm.io/driver/sqlserver"
	"gorm.io/gorm"
)

type DBDriver struct {
	DBConfig *configs.DBConfigs
	DB       *gorm.DB
}

func (d *DBDriver) CreateByTenant(tenant string) error {
	dbc, err := configs.GetDBConfigs(tenant)
	if err != nil {
		return err
	}

	return d.Create(dbc)
}

func (d *DBDriver) Create(db *configs.DBConfigs) error {
	if db == nil {
		return errors.New("database configuration is required")
	}

	d.DBConfig = db
	return d.Connect()
}

func (d *DBDriver) Connect() error {
	db, error := d.newConnection()
	if error != nil {
		return error
	}
	sqlDB, err := db.DB()
	if err != nil {
		log.Fatalf("Failed to extract sql.DB instance: %v", err)
		return err
	}

	// 2. Create a context with a strict timeout for the ping operation
	ctx, cancel := context.WithTimeout(context.Background(), 2*time.Second)
	defer cancel()

	// 3. Ping the database to verify the network connection is alive
	if err := sqlDB.PingContext(ctx); err != nil {
		log.Fatalf("Database connection is dead: %v", err)
		return err
	}
	sqlDB.SetMaxOpenConns(50)
	sqlDB.SetMaxIdleConns(10)
	sqlDB.SetConnMaxLifetime(time.Hour)
	sqlDB.SetConnMaxIdleTime(10 * time.Minute)
	d.DB = db
	return nil
}

func (d *DBDriver) CloseConnection() error {

	if sqlDB, err := (*d.DB).DB(); err == nil {
		return sqlDB.Close()
	}
	return nil
}

func (d *DBDriver) ExecuteCommandTransaction(command string) error {
	return nil
}

func (d *DBDriver) newConnection() (*gorm.DB, error) {
	dbConfig := d.DBConfig
	if dbConfig == nil {
		panic("Error in connections")
	}
	switch dbConfig.Drivers {
	case "mysql":
		dsn := dbConfig.User + ":" + dbConfig.Password + "@tcp(" + dbConfig.Host + ":" + dbConfig.Port + ")/" + dbConfig.DBName + "?parseTime=true"
		db, err := gorm.Open(mysql.Open(dsn), &gorm.Config{})
		if err != nil {
			return nil, err
		}

		return db, nil
	case "postgres":
		dsn := "host=" + dbConfig.Host + " port=" + dbConfig.Port + " user=" + dbConfig.User + " password=" + dbConfig.Password + " dbname=" + dbConfig.DBName + " sslmode=disable"
		db, err := gorm.Open(postgres.Open(dsn), &gorm.Config{})
		if err != nil {
			return nil, err
		}

		return db, nil
	case "sqlite":
		db, err := gorm.Open(sqlite.Open(dbConfig.DBName), &gorm.Config{})
		if err != nil {
			return nil, err
		}

		return db, nil
	case "mssql":
		dsn := "sqlserver://" + dbConfig.User + ":" + dbConfig.Password + "@" + dbConfig.Host + ":" + dbConfig.Port + "?database=" + dbConfig.DBName
		db, err := gorm.Open(sqlserver.Open(dsn), &gorm.Config{})
		if err != nil {
			return nil, err
		}

		return db, nil
	case "oracle":
		dsn := dbConfig.User + "/" + dbConfig.Password + "@" + dbConfig.Host + ":" + dbConfig.Port + "/" + dbConfig.DBName
		return drivers.OpenOracle(dsn)
	default:
		return nil, errors.New("unsupported database driver: " + dbConfig.Drivers)
	}

}
