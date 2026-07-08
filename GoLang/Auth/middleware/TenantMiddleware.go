package middleware

import (
	"net/http"

	"github.com/gin-gonic/gin"
)

const TenantKey = "tenant"

func TenantMiddleware() gin.HandlerFunc {
	return func(c *gin.Context) {

		tenant := c.Param("tenant")

		if tenant == "" {
			c.AbortWithStatusJSON(http.StatusBadRequest, gin.H{
				"error": "tenant is required",
			})
			return
		}

		// Optional:
		// Validate tenant from database/cache here.

		c.Set(TenantKey, tenant)

		c.Next()
	}
}
