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
	UserID   int64    `json:"userid"`
	Tenant   string   `json:"tenant"`
	Roles    []string `json:"roles"`
	// Role is retained while clients transition from the previous singular claim.
	Role     []string `json:"role,omitempty"`
	Username string   `json:"username"`

	jwt.RegisteredClaims
}
