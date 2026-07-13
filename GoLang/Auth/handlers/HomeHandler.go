package handlers

import (
	"auth-go/middleware"

	"github.com/gin-gonic/gin"
)

type TenantHandler struct {
	Tenant string // Add any necessary fields or dependencies here
}

func (h *TenantHandler) Route(authorizedTenant, authRouterGroup *gin.RouterGroup) {

	authorizedTenant.Use(middleware.ValidateRequest(), middleware.TenantMiddleware())
	{
		(*h).RegisterRouteAuth(authorizedTenant)
		/*authorizedTenantApi.GET("/protected", func(c *gin.Context) {
			tenant := c.MustGet(middleware.TenantKey).(string)
			c.JSON(200, gin.H{
				"message": "Hello, " + tenant + "! You have access to this protected route.",
			})
		})*/
	}

	authRouterGroup.Use(middleware.ValidateRequest(), middleware.TenantMiddleware())
	{
		(*h).RegisterRoute(authRouterGroup)

	}
}

func (h *TenantHandler) RegisterRouteAuth(router *gin.RouterGroup) {
	//router.POST("/auth/validate", h.Validate)
	//router.POST("/auth/validate", h.Logout)
}

func (h *TenantHandler) RegisterRoute(router *gin.RouterGroup) {
	//router.POST("/auth/login", h.Login)
}
