package cache

import (
	"encoding/json"
	"fmt"
	"strconv"
	"time"

	"auth-go/configs"
	"auth-go/extensions"

	"github.com/go-redis/redis"
)

var defaultTimeout time.Duration = 8 * time.Hour

var RDBCache *redis.Client

const maxDataArraySize = 500

// SaveData serializes data to JSON and stores it in Redis with the given timeout.
// Use a timeout of 0 for a key that does not expire.
func SaveData(key string, data any, timeout *time.Duration) error {
	if timeout == nil {
		timeout = &defaultTimeout
	}

	jsonData, err := json.Marshal(data)
	if err != nil {
		return err
	}

	return RDBCache.Set(key, string(jsonData), *timeout).Err()
}

// GetData retrieves JSON data from Redis, unmarshals it into T, and refreshes
// the key expiration. A nil timeout defaults to eight hours.
func GetData[T any](key string, timeout *time.Duration) (T, error) {
	var data T

	if timeout == nil {
		timeout = &defaultTimeout
	}

	jsonData, err := RDBCache.Get(key).Result()
	if err != nil {
		return data, err
	}

	if err := RDBCache.Expire(key, *timeout).Err(); err != nil {
		return data, err
	}

	if err := json.Unmarshal([]byte(jsonData), &data); err != nil {
		return data, err
	}

	return data, nil
}

// SetDataArray splits objects into JSON arrays of at most 500 items. It stores
// the number of arrays at key and each array at key followed by its zero-based
// index (for example, users0, users1, and so on).
func SetDataArray[T any](key string, objects []T, timeout *time.Duration) error {
	if timeout == nil {
		timeout = &defaultTimeout
	}
	if len(objects) == 0 {
		return SaveData(key, 0, timeout)
	}

	dataArrays := extensions.SplitArray(objects, maxDataArraySize)
	arrayCount := len(dataArrays)
	if err := SaveData(key, arrayCount, timeout); err != nil {
		return err
	}

	errors := make(chan error, arrayCount)
	for index, dataArray := range dataArrays {
		go func(index int, dataArray []T) {
			chunkKey := key + strconv.Itoa(index)
			errors <- SaveData(chunkKey, dataArray, timeout)
		}(index, dataArray)
	}

	for range dataArrays {
		if err := <-errors; err != nil {
			return err
		}
	}

	return nil
}

// GetDataArray retrieves JSON arrays stored by SetDataArray and combines them
// into one slice. Each chunk is read concurrently while preserving its order.
func GetDataArray[T any](key string, timeout *time.Duration) ([]T, error) {
	if timeout == nil {
		timeout = &defaultTimeout
	}

	arrayLength, err := GetData[int](key, timeout)
	if err != nil {
		return nil, err
	}
	if arrayLength < 0 {
		return nil, fmt.Errorf("invalid cached array length for key %q", key)
	}
	if arrayLength == 0 {
		return []T{}, nil
	}

	type arrayResult struct {
		index int
		data  []T
		err   error
	}

	results := make(chan arrayResult, arrayLength)
	for index := 0; index < arrayLength; index++ {
		go func(index int) {
			data, err := GetData[[]T](key+strconv.Itoa(index), timeout)
			results <- arrayResult{index: index, data: data, err: err}
		}(index)
	}

	dataArrays := make([][]T, arrayLength)
	var firstErr error
	for range arrayLength {
		result := <-results
		if result.err != nil {
			if firstErr == nil {
				firstErr = result.err
			}
			continue
		}
		dataArrays[result.index] = result.data
	}
	if firstErr != nil {
		return nil, firstErr
	}

	data := make([]T, 0)
	for _, dataArray := range dataArrays {
		data = append(data, dataArray...)
	}

	return data, nil
}

func Load() {
	redisConfig, err1 := configs.AppConfig.GetValue("Redis").(map[string]any)
	if redisConfig == nil || err1 {
		return
	}
	RDBCache = redis.NewClient(&redis.Options{
		Addr:     fmt.Sprintf("%s:%s", redisConfig["redis_addr"].(string), redisConfig["redis_port"].(string)),
		Password: redisConfig["redis_password"].(string),
		DB:       0, // Default DB
	})

	err := RDBCache.Ping().Err()
	if err != nil {
		panic(err)
	}

	fmt.Println("Connected to Redis")
}

func IsJWTExpired(jwtKey string) bool {
	key := extensions.Format(JWTAuthKeyExpired, map[string]string{"key": jwtKey})
	data, err := GetData[bool](key, nil)
	if err == nil {
		return false
	}
	return data
}

func setJWTExpired(jwtKey string) {
	key := extensions.Format(JWTAuthKeyExpired, map[string]string{"key": jwtKey})
	SaveData(key, false, nil)
}
