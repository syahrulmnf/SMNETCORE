package cache

import (
	"fmt"

	"auth-go/configs"

	"github.com/go-redis/redis"
)

var rdb *redis.Client

func Load() {
	redisConfig, err1 := configs.AppConfig.GetValue("Redis").(map[string]any)
	if redisConfig == nil || err1 {
		return
	}
	rdb = redis.NewClient(&redis.Options{
		Addr:     fmt.Sprintf("%s:%s", redisConfig["redis_addr"].(string), redisConfig["redis_port"].(string)),
		Password: redisConfig["redis_password"].(string),
		DB:       0, // Default DB
	})

	err := rdb.Ping().Err()
	if err != nil {
		panic(err)
	}

	fmt.Println("Connected to Redis")
}
