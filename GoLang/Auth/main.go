package main

import (
	"auth-go/common/cache"
	"auth-go/configs"
	"auth-go/handlers"

	"github.com/gin-gonic/gin"
)

func main() {
	configs.AppConfig = &configs.AppConfigs{}
	configs.AppConfig.Loads()
	cache.Load()

	router := gin.Default()
	authorizedTenant := router.Group("/:tenant")
	authRouterGroup := router.Group("/r/")

	jwtHandler := &handlers.JWTHandler{}
	jwtHandler.Route(authorizedTenant, authRouterGroup)

	router.Run(":8080")
}
