package configs

import (
	"encoding/json"
	"os"
)

type AppConfigs struct {
	Keys   []string       `json:"available_keys"`
	Values map[string]any `json:"values"`
}

func (c *AppConfigs) GetValue(key string) any {
	if val, ok := c.Values[key]; ok {
		return val
	}
	return nil
}

func (c *AppConfigs) GetKeys() []string {
	return c.Keys
}

func (c *AppConfigs) SetValue(key string, value any) {
	if c.Values == nil {
		c.Values = make(map[string]any)
	}
	c.Values[key] = value
}

func (c *AppConfigs) SetKeys(keys []string) {
	c.Keys = keys
}

func (c *AppConfigs) AddKey(key string, value any) {
	c.Keys = append(c.Keys, key)
	c.Values[key] = value
}

func (c *AppConfigs) RemoveKey(key string) {
	for i, k := range c.Keys {
		if k == key {
			c.Keys = append(c.Keys[:i], c.Keys[i+1:]...)
			break
		}
		delete(c.Values, key)
	}
}

func (c *AppConfigs) HasKey(key string) bool {
	_, exists := c.Values[key]
	return exists
}

func (c *AppConfigs) GetAll() map[string]any {
	return c.Values
}

func (c *AppConfigs) Clear() {
	c.Keys = []string{}
	c.Values = make(map[string]any)
}

func (c *AppConfigs) UpdateValue(key string, value any) {
	if c.HasKey(key) {
		c.Values[key] = value
	}
}

func (c *AppConfigs) UpdateKeys(keys []string) {
	c.Keys = keys
}

func (c *AppConfigs) Update(key string, value any) {
	if c.HasKey(key) {
		c.Values[key] = value
	} else {
		c.AddKey(key, value)
	}
}

func (c *AppConfigs) Loads() AppConfigs {
	// Load configurations from a source (e.g., file, environment variables)
	// This is a placeholder for actual loading logic.
	path := "./files/appconfigs.json"
	if _, err := os.Stat(path); os.IsExist(err) {
		// If the file does not exist, create it with default values
		var mapped map[string]any
		data, err := os.ReadFile(path)

		if err != nil {
			return *c
		}

		if err := json.Unmarshal(data, &mapped); err != nil {
			return *c
		}

		c = &AppConfigs{
			Keys:   []string{},
			Values: make(map[string]any),
		}

		c.Keys = []string{}
		c.Values = make(map[string]any)
		for key, value := range mapped {
			c.Keys = append(c.Keys, key)
			c.Values[key] = value
		}

	}
	return *c
}

var AppConfig = &AppConfigs{
	Keys:   []string{},
	Values: make(map[string]any),
}
