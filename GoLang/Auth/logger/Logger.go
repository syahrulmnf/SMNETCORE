package logger

import (
	"context"
	"io"
	"log/slog"

	"gopkg.in/natefinch/lumberjack.v2"
)

type LevelRouter struct {
	info  slog.Handler
	warn  slog.Handler
	error slog.Handler
}

var Logger *slog.Logger

func newRollingFile(filename string) io.Writer {
	return &lumberjack.Logger{
		Filename:   filename,
		MaxSize:    100,
		MaxBackups: 10,
		MaxAge:     30,
		Compress:   true,
	}
}

func NewLogger() *slog.Logger {
	router := &LevelRouter{
		info: slog.NewJSONHandler(
			newRollingFile("logs/info.log"),
			&slog.HandlerOptions{
				Level:     slog.LevelInfo,
				AddSource: true,
				ReplaceAttr: func(groups []string, a slog.Attr) slog.Attr {
					if a.Key == slog.TimeKey {
						return slog.String(
							"time",
							a.Value.Time().Local().Format("2006-01-02 15:04:05"),
						)
					}
					return a
				},
			},
		),
		warn: slog.NewJSONHandler(
			newRollingFile("logs/warn.log"),
			&slog.HandlerOptions{
				Level:     slog.LevelWarn,
				AddSource: true,
				ReplaceAttr: func(groups []string, a slog.Attr) slog.Attr {
					if a.Key == slog.TimeKey {
						return slog.String(
							"time",
							a.Value.Time().Local().Format("2006-01-02 15:04:05"),
						)
					}
					return a
				},
			},
		),
		error: slog.NewJSONHandler(
			newRollingFile("logs/error.log"),
			&slog.HandlerOptions{
				Level:     slog.LevelError,
				AddSource: true,
				ReplaceAttr: func(groups []string, a slog.Attr) slog.Attr {
					if a.Key == slog.TimeKey {
						return slog.String(
							"time",
							a.Value.Time().Local().Format("2006-01-02 15:04:05"),
						)
					}
					return a
				},
			},
		),
	}

	return slog.New(router)
}

func (h *LevelRouter) Enabled(ctx context.Context, level slog.Level) bool {
	return true
}

func (h *LevelRouter) Handle(ctx context.Context, r slog.Record) error {

	switch {
	case r.Level >= slog.LevelError:
		return h.error.Handle(ctx, r)

	case r.Level >= slog.LevelWarn:
		return h.warn.Handle(ctx, r)

	default:
		return h.info.Handle(ctx, r)
	}
}

func (h *LevelRouter) WithAttrs(attrs []slog.Attr) slog.Handler {
	return &LevelRouter{
		info:  h.info.WithAttrs(attrs),
		warn:  h.warn.WithAttrs(attrs),
		error: h.error.WithAttrs(attrs),
	}
}

func (h *LevelRouter) WithGroup(name string) slog.Handler {
	return &LevelRouter{
		info:  h.info.WithGroup(name),
		warn:  h.warn.WithGroup(name),
		error: h.error.WithGroup(name),
	}
}
func init() {
	Logger = NewLogger()
}
