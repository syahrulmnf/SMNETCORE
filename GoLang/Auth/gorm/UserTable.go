package gorm

import "time"

type UserLoginTable struct {
	Id           int64 `gorm:"PrimaryKey;"`
	DisplayName  string
	FirstName    string
	MiddleName   string
	LastName     string
	EmailAddress string
	Salt         string
	Password     string
	Created      time.Time
	Modified     time.Time
	Expired      time.Time
	Version      int
	AllowLogin   bool
	Deleted      bool
}

type UserTenantLoginTable struct {
	Id         int64 `gorm:"PrimaryKey;"`
	UserId     int64
	TenantId   int64
	Deleted    bool
	Active     bool
	AllowLogin bool
}

type UserRoles struct {
	Id       int64 `gorm:"PrimaryKey;"`
	TenantId int64
	Roles    string
	UserId   int64
}
