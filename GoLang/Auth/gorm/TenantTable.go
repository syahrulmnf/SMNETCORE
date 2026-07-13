package gorm

import "time"

type TenantTable struct {
	Id      int64 `gorm:"PrimaryKey;"`
	Name    string
	Domain  string
	Secret  string
	AdminId int64
}

var SubscriptionsType = map[string]string{
	"Default": "Default",
}

type TenantServicesSubscriptions struct {
	Id       int64 `gorm:"PrimaryKey;"`
	TenantId int64
	SubsType string
	Active   bool
	Deleted  bool
	Created  time.Time
	Modified time.Time
}

type TennantServiceSubsParams struct {
	ParamsId     int64 `gorm:"PrimaryKey;"`
	TenantSubsUd int64
	Name         string
	Value        string
	Type         int
	IsBody       bool
}
