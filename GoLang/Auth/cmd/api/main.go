package main

import (
	"auth-go/cache"
	"auth-go/configs"
	"auth-go/handlers"

	"github.com/gin-gonic/gin"
)

func main() {
	configs.AppConfig = &configs.AppConfigs{}
	configs.AppConfig.Loads()
	configs.LoadDBConfigs()
	cache.Load()

	router := gin.Default()
	authorizedTenant := router.Group("/:tenant")

	jwtHandler := &handlers.JWTHandler{}
	jwtHandler.Route(authorizedTenant)

	router.Run(":8080")
}
