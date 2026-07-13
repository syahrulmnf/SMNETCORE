package gorm

import "time"

var AuthType = map[string]string{
	"Login":      "Login",
	"MobileApp":  "MobileApp",
	"Hook":       "Hook",
	"ThirdParty": "ThirdParty",
}

type LoginCred struct {
	Id       int64 `gorm:"PrimaryKey;column:id"`
	Tenant   string
	TenantId int64
	UserId   int64
	Secret   string
	Tokens   string
	Claims   string
	Created  time.Time
	Expired  time.Time
	IsExpired bool
	Deleted  bool
	Active   bool
	AppName  string
	AuthType string
}
