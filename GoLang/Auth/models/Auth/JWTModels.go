package models

import "github.com/golang-jwt/jwt/v5"

type JWTAuthRequestModel struct {
	Token string `json:"token"`
}

type AuthRequestModel struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

type JWTClaimsModel struct {
	UserID   int      `json:"userid"`
	Tenant   string   `json:"tenant"`
	Role     []string `json:"role"`
	Username string   `json:"username"`

	jwt.RegisteredClaims
}
