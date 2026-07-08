package jwt

import (
	"auth-go/configs"
	"auth-go/logger"
	models "auth-go/models/Auth"

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
		if claims == nil || tenant == "" || len((*claims).Role) == 0 {
			c.AbortWithStatusJSON(401, gin.H{
				"message": "Unauthorized",
			})
			return
		}

		for _, p := range (*claims).Role {
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

func ValidateRequest() gin.HandlerFunc {
	return func(c *gin.Context) {
		tenant := c.MustGet("tenant").(string)
		header := c.GetHeader("Authorization")
		secret := configs.AppConfig.Values["JWT"].(map[string]string)["jwt_secret"]
		if tenant == "" && header == "" {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{
				"message": "missing authorization",
			})
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

			return secret, nil
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
