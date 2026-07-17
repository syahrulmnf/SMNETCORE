package handlers

import (
	"auth-go/cache"
	"auth-go/common"
	"auth-go/configs"
	authgorm "auth-go/gorm"
	"auth-go/middleware"
	models "auth-go/models/Auth"
	"auth-go/repositories"
	"encoding/json"
	"errors"
	"net/http"
	"strings"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
	"gorm.io/gorm"
)

type JWTHandler struct{}

// Route registers public login and JWT-protected endpoints below /:tenant.
func (h *JWTHandler) Route(tenantRoutes *gin.RouterGroup) {
	publicRoutes := tenantRoutes.Group("")
	publicRoutes.Use(middleware.TenantMiddleware())
	h.RegisterRoute(publicRoutes)

	protectedRoutes := tenantRoutes.Group("")
	protectedRoutes.Use(middleware.TenantMiddleware(), middleware.ValidateRequest())
	h.RegisterRouteAuth(protectedRoutes)
}

func (h *JWTHandler) RegisterRouteAuth(router *gin.RouterGroup) {
	router.POST("/auth/validate", h.Validate)
	router.POST("/auth/logout", h.Logout)
}

func (h *JWTHandler) RegisterRoute(router *gin.RouterGroup) {
	router.POST("/auth/login", h.Login)
}

func (h *JWTHandler) Validate(c *gin.Context) {
	claims, ok := c.Get(middleware.ClaimsKey)
	if !ok {
		c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"message": "Unauthorized"})
		return
	}
	c.JSON(http.StatusOK, gin.H{"valid": true, "claims": claims})
}

func (h *JWTHandler) Logout(c *gin.Context) {
	claims, ok := c.Get(middleware.ClaimsKey)
	if !ok {
		c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Unauthorized"})
		return
	}
	jwtClaims, ok := claims.(*models.JWTClaimsModel)
	if !ok || jwtClaims == nil {
		c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "Unauthorized"})
		return
	}

	tenant := c.GetString(middleware.TenantKey)
	token := strings.TrimPrefix(c.GetHeader("Authorization"), "Bearer ")
	usersRepo := &repositories.UsersRepo{}
	if err := usersRepo.CreateByTenant(tenant); err != nil {
		c.JSON(http.StatusServiceUnavailable, gin.H{"error": "Authentication service is unavailable"})
		return
	}
	expired := make(chan bool)
	go func(t string) {
		erExp := cache.SetJWTExpired(token)
		if erExp == nil {
			expired <- true
		} else {
			expired <- false
		}
	}(token)

	if err := usersRepo.ExpireLoginCred(token, tenant, jwtClaims.UserID); err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			c.Status(http.StatusNoContent)
			return
		}
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Could not log out"})
		return
	}
	c.Status(http.StatusNoContent)
}

func (h *JWTHandler) Login(c *gin.Context) {
	var request models.AuthRequestModel
	if err := c.ShouldBindJSON(&request); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid request payload"})
		return
	}
	if request.Username == "" || request.Password == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Username and password are required"})
		return
	}
	genericRepo := &repositories.GenericRepo{}
	tenant := c.GetString(middleware.TenantKey)
	if err := genericRepo.CreateByTenant(); err == nil && tenant == common.GlobalsData.GeneralTenant {
		tenantT, errT := genericRepo.GetGenericTenant(request.Username)
		if errT != nil {
			c.JSON(http.StatusServiceUnavailable, gin.H{"error": "Authentication service is unavailable"})
			return
		}
		tenant = tenantT
	}

	usersRepo := &repositories.UsersRepo{}
	if err := usersRepo.CreateByTenant(tenant); err != nil {
		c.JSON(http.StatusServiceUnavailable, gin.H{"error": "Authentication service is unavailable"})
		return
	}
	user, err := usersRepo.VerifyLogin(request.Username, request.Password, tenant)
	if err != nil {
		if errors.Is(err, repositories.ErrInvalidCredentials) || errors.Is(err, repositories.ErrUserNotAllowedForTenant) {
			c.JSON(http.StatusUnauthorized, gin.H{"error": "Invalid username or password"})
			return
		}
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Could not verify login"})
		return
	}
	roles, err := usersRepo.GetRoles(user.Id, tenant)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Could not load user roles"})
		return
	}

	tenantRepo := repositories.NewTenantRepo(usersRepo.Driver)
	tenantRecord, err := tenantRepo.GetTenant(tenant)
	if err != nil {
		c.JSON(http.StatusServiceUnavailable, gin.H{"error": "Tenant is not configured"})
		return
	}

	jwtConfig, _ := configs.ConfigMap("JWT")
	fallbackSecret := configs.ConfigString(jwtConfig, "jwt_secret")
	secret := tenantRecord.Secret
	if secret == "" {
		secret = fallbackSecret
	}
	if secret == "" {
		c.JSON(http.StatusServiceUnavailable, gin.H{"error": "JWT is not configured"})
		return
	}

	expiresIn := configs.ConfigDuration(jwtConfig, "jwt_expiration", time.Hour)
	now := time.Now()
	claims := models.JWTClaimsModel{
		UserID:   user.Id,
		Tenant:   tenant,
		Roles:    roles,
		Username: user.EmailAddress,
		RegisteredClaims: jwt.RegisteredClaims{
			Subject: user.EmailAddress, Issuer: "auth-go", IssuedAt: jwt.NewNumericDate(now),
			ExpiresAt: jwt.NewNumericDate(now.Add(expiresIn)),
		},
	}
	if claims.Username == "" {
		claims.Username = user.DisplayName
		claims.Subject = user.DisplayName
	}
	token, err := jwt.NewWithClaims(jwt.SigningMethodHS256, claims).SignedString([]byte(secret))
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Could not create token"})
		return
	}
	claimsJSON, err := json.Marshal(claims)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Could not save login"})
		return
	}
	loginCred := &authgorm.LoginCred{
		Tenant:    tenant,
		TenantId:  tenantRecord.Id,
		UserId:    user.Id,
		Secret:    secret,
		Tokens:    token,
		Claims:    string(claimsJSON),
		Created:   now,
		Expired:   now.Add(expiresIn),
		IsExpired: false,
		Active:    true,
		AppName:   "Web",
		AuthType:  authgorm.AuthType["Login"],
		Roles:     strings.Join(roles, ","),
	}
	if err := usersRepo.SaveLoginCred(loginCred); err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Could not save login"})
		return
	}
	c.JSON(http.StatusOK, gin.H{"access_token": token, "token_type": "Bearer", "expires_in": int(expiresIn.Seconds())})
}
