package repositories

import (
	"context"
	"log"
	"time"

	"github.com/oracle-samples/gorm-oracle/oracle"
	"gorm.io/driver/mysql"
	"gorm.io/driver/postgres"
	"gorm.io/driver/sqlite"
	"gorm.io/driver/sqlserver"
	"gorm.io/gorm"
)

type DBDriver struct {
	DriverName   string
	Host         string
	Port         int
	Username     string
	Password     string
	DatabaseName string
	DB           *gorm.DB
}

var DBDriversType []string = []string{
	"mysql",
	"postgres",
	"sqlite",
	"mssql",
	"oracle",
}

func (d *DBDriver) Connect() (*gorm.DB, error) {
	db, error := d.newConnection()
	if error != nil {
		return nil, error
	}
	sqlDB, err := db.DB()
	if err != nil {
		log.Fatalf("Failed to extract sql.DB instance: %v", err)
	}

	// 2. Create a context with a strict timeout for the ping operation
	ctx, cancel := context.WithTimeout(context.Background(), 2*time.Second)
	defer cancel()

	// 3. Ping the database to verify the network connection is alive
	if err := sqlDB.PingContext(ctx); err != nil {
		log.Fatalf("Database connection is dead: %v", err)
	}
	sqlDB.SetMaxOpenConns(50)
	sqlDB.SetMaxIdleConns(10)
	sqlDB.SetConnMaxLifetime(time.Hour)
	sqlDB.SetConnMaxIdleTime(10 * time.Minute)
	return db, nil
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
	switch d.DriverName {
	case "mysql":
		dsn := d.Username + ":" + d.Password + "@tcp(" + d.Host + ":" + string(d.Port) + ")/" + d.DatabaseName + "?parseTime=true"
		db, err := gorm.Open(mysql.Open(dsn), &gorm.Config{})
		if err != nil {
			return nil, err
		}

		return db, nil
	case "postgres":
		dsn := "host=" + d.Host + " port=" + string(d.Port) + " user=" + d.Username + " password=" + d.Password + " dbname=" + d.DatabaseName + " sslmode=disable"
		db, err := gorm.Open(postgres.Open(dsn), &gorm.Config{})
		if err != nil {
			return nil, err
		}

		return db, nil
	case "sqlite":
		db, err := gorm.Open(sqlite.Open(d.DatabaseName), &gorm.Config{})
		if err != nil {
			return nil, err
		}

		return db, nil
	case "mssql":
		dsn := "sqlserver://" + d.Username + ":" + d.Password + "@" + d.Host + ":" + string(d.Port) + "?database=" + d.DatabaseName
		db, err := gorm.Open(sqlserver.Open(dsn), &gorm.Config{})
		if err != nil {
			return nil, err
		}

		return db, nil
	case "oracle":
		dsn := d.Username + "/" + d.Password + "@" + d.Host + ":" + string(d.Port) + "/" + d.DatabaseName
		db, err := gorm.Open(oracle.Open(dsn), &gorm.Config{})
		if err != nil {
			return nil, err
		}

		return db, nil
	default:
		return nil, nil
	}

}
