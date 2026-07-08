package handlers

import (
	"auth-go/jwt"
	"auth-go/middleware"
	models "auth-go/models/Auth"

	"github.com/gin-gonic/gin"
)

type JWTHandler struct {
	tenant string
}

func (h *JWTHandler) Route(authorizedTenant, authRouterGroup *gin.RouterGroup) {

	authorizedTenant.Use(jwt.ValidateRequest(), middleware.TenantMiddleware())
	{
		(*h).RegisterRouteAuth(authorizedTenant)
		/*authorizedTenantApi.GET("/protected", func(c *gin.Context) {
			tenant := c.MustGet(middleware.TenantKey).(string)
			c.JSON(200, gin.H{
				"message": "Hello, " + tenant + "! You have access to this protected route.",
			})
		})*/
	}

	authRouterGroup.Use(jwt.ValidateRequest(), middleware.TenantMiddleware())
	{
		(*h).RegisterRoute(authRouterGroup)

	}
}

func (h *JWTHandler) RegisterRouteAuth(router *gin.RouterGroup) {
	router.POST("/auth/validate", h.Validate)
	router.POST("/auth/validate", h.Logout)
}

func (h *JWTHandler) RegisterRoute(router *gin.RouterGroup) {
	router.POST("/auth/login", h.Login)
}

func (h *JWTHandler) Validate(c *gin.Context) {
}

func (h *JWTHandler) Logout(c *gin.Context) {
}

func (h *JWTHandler) Login(c *gin.Context) {
	(*h).tenant = c.MustGet(middleware.TenantKey).(string)

	var requestModel *models.AuthRequestModel
	if err := c.ShouldBindJSON(&requestModel); err != nil {
		c.JSON(400, gin.H{
			"error": "Invalid request payload",
		})
		return
	}
	if (*requestModel).Username == "" || (*requestModel).Password == "" {
		c.JSON(400, gin.H{
			"error": "Username and password are required",
		})
		return
	}

}
