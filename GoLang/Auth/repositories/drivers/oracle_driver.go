//go:build !oracle

package drivers

import (
	"errors"

	"gorm.io/gorm"
)

func OpenOracleErrors(string) (*gorm.DB, error) {
	return nil, errors.New("oracle support is disabled; build with -tags oracle")
}
