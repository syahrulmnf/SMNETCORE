package extensions

// SplitArray splits data into arrays containing at most arraySize items each.
// It returns an empty result when arraySize is zero or negative.
func SplitArray[T any](data []T, arraySize int) [][]T {
	if arraySize <= 0 {
		return [][]T{}
	}

	result := make([][]T, 0, (len(data)+arraySize-1)/arraySize)
	for start := 0; start < len(data); start += arraySize {
		end := start + arraySize
		if end > len(data) {
			end = len(data)
		}

		result = append(result, data[start:end])
	}

	return result
}
