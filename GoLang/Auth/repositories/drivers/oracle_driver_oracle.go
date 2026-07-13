package drivers

import (
	"github.com/oracle-samples/gorm-oracle/oracle"
	"gorm.io/gorm"
)

func OpenOracle(dsn string) (*gorm.DB, error) {
	return gorm.Open(oracle.Open(dsn), &gorm.Config{})
}
