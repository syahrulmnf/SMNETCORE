package repositories

import "errors"

var (
	ErrUserRepositoryNotInitialized   = errors.New("user repository is not initialized")
	ErrInvalidCredentials             = errors.New("invalid username or password")
	ErrGenericTenantInvalidCredentials = errors.New("Invalid credentials")
	ErrUserNotAllowedForTenant        = errors.New("user is not allowed to log in to this tenant")
)
