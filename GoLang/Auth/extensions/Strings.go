package extensions

import (
	"encoding/json"
	"strconv"
	"strings"
)

// SplitString splits a separator-delimited string, trimming whitespace and
// omitting blank or duplicate values.
func SplitString(value, separator string) []string {
	roles := make([]string, 0)
	seen := make(map[string]struct{})
	for _, role := range strings.Split(value, separator) {
		role = strings.TrimSpace(role)
		if role == "" {
			continue
		}
		if _, exists := seen[role]; exists {
			continue
		}
		seen[role] = struct{}{}
		roles = append(roles, role)
	}
	return roles
}

func Format(template string, values map[string]string) string {
	for k, v := range values {
		template = strings.ReplaceAll(template, "{"+k+"}", v)
	}
	return template
}
func ToString(value any) string {
	switch value := value.(type) {
	case string:
		return value
	case float64:
		return strconv.FormatFloat(value, 'f', -1, 64)
	case float32:
		return strconv.FormatFloat(float64(value), 'f', -1, 32)
	case int:
		return strconv.Itoa(value)
	case int64:
		return strconv.FormatInt(value, 10)
	case json.Number:
		return value.String()
	default:
		return ""
	}
}

func MapValueToString(data map[string]any, key string) string {
	value, exists := data[key]
	if !exists || value == nil {
		return ""
	}
	return strings.TrimSpace(ToString(value))
}
