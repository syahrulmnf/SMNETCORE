package extensions

import "strings"

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
