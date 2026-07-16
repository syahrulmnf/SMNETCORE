package repositories

/*
import (
	"auth-go/configs"
	gormmodel "auth-go/gorm"

	"gorm.io/gorm"
)

type CreateUsers struct {
	Driver *DBDriver
}

func (h *CreateUsers) NewRepo(driver *DBDriver) {
	h = &CreateUsers{Driver: driver}
}

func (u *CreateUsers) CreateByTenant(tenant string) error {
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

func (u *CreateUsers) CreateRepo(db *configs.DBConfigs) error {
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

func (u *CreateUsers) DBRepo() (*gorm.DB, error) {
	if u == nil || u.Driver == nil || u.Driver.DB == nil {
		return nil, ErrUserRepositoryNotInitialized
	}
	return u.Driver.DB, nil
}

func (u *CreateUsers) Create(user *gormmodel.UserLoginTable) (*gormmodel.UserLoginTable, error) {
	return user, nil
}
func (u *CreateUsers) Update(user *gormmodel.UserLoginTable) (*gormmodel.UserLoginTable, error) {
	return user, nil
}
func (u *CreateUsers) Delete(user *gormmodel.UserLoginTable) (*gormmodel.UserLoginTable, error) {
	return user, nil
}
*/
