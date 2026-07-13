package middleware

import (
	"auth-go/configs"
	"auth-go/logger"
	models "auth-go/models/Auth"
	"auth-go/repositories"

	"errors"
	"net/http"
	"slices"
	"strings"

	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
)

const ClaimsKey = "claims"

func RequirePermission(roles []string) gin.HandlerFunc {

	return func(c *gin.Context) {
		tenant := c.Param("tenant")
		claims := c.MustGet(ClaimsKey).(*models.JWTClaimsModel)
		if claims == nil || tenant == "" || len(claimRoles(*claims)) == 0 {
			c.AbortWithStatusJSON(401, gin.H{
				"message": "Unauthorized",
			})
			return
		}

		for _, p := range claimRoles(*claims) {
			if slices.Contains(roles, p) {
				c.Next()
				return
			}
		}

		c.AbortWithStatusJSON(http.StatusForbidden, gin.H{
			"message": "Permission denied",
		})
	}
}

// claimRoles accepts tokens created before the roles claim was introduced.
func claimRoles(claims *models.JWTClaimsModel) []string {
	if len(claims.Roles) != 0 {
		return claims.Roles
	}
	return claims.Role
}

func ValidateRequest() gin.HandlerFunc {
	return func(c *gin.Context) {
		tenant := c.GetString(TenantKey)
		header := c.GetHeader("Authorization")
		jwtConfig, ok := configs.AppConfig.Values["JWT"].(map[string]any)
		if !ok {
			c.AbortWithStatusJSON(http.StatusServiceUnavailable, gin.H{"message": "JWT is not configured"})
			return
		}
		secret, _ := jwtConfig["jwt_secret"].(string)
		if tenant == "" || header == "" {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{
				"message": "missing authorization",
			})
			return
		}

		dbc, err := configs.GetDBConfigs(tenant)
		if err != nil {
			c.AbortWithStatusJSON(http.StatusServiceUnavailable, gin.H{"message": "Tenant is not configured"})
			return
		}
		tenantRepo := &repositories.TenantRepo{}
		if err := tenantRepo.Create(dbc); err != nil {
			c.AbortWithStatusJSON(http.StatusServiceUnavailable, gin.H{"message": "Authentication service is unavailable"})
			return
		}
		if _, err := tenantRepo.GetTenant(tenant); err == nil && tenantRepo.Secret != "" {
			secret = tenantRepo.Secret
		}
		if secret == "" {
			c.AbortWithStatusJSON(http.StatusServiceUnavailable, gin.H{"message": "JWT is not configured"})
			return
		}

		if !strings.HasPrefix(header, "Bearer ") {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{
				"message": "invalid authorization",
			})
			return
		}

		token := strings.TrimPrefix(header, "Bearer ")

		claims, err := ValidateToken(token, secret)
		if err != nil {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{
				"message": "invalid authorization",
			})
			logger.Logger.Error("Error validating token", "Error:", err.Error())
			return
		}
		if claims.Tenant != tenant {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{
				"message": "invalid authorization",
			})
			return
		}

		c.Set(ClaimsKey, claims)

		c.Next()
	}
}

func ValidateToken(tokenString string, secret string) (*models.JWTClaimsModel, error) {

	token, err := jwt.ParseWithClaims(
		tokenString,
		&models.JWTClaimsModel{},
		func(token *jwt.Token) (interface{}, error) {

			if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
				return nil, errors.New("unexpected signing method")
			}

			return []byte(secret), nil
		},
	)

	if err != nil {
		return nil, err
	}

	claims, ok := token.Claims.(*models.JWTClaimsModel)
	if !ok || !token.Valid {
		return nil, errors.New("invalid token")
	}

	return claims, nil
}
