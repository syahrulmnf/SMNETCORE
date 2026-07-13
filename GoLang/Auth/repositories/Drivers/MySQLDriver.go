package drivers

import "database/sql"

func New() (*sql.DB, error) {

	dsn := "root:password@tcp(localhost:3306)/mydb?parseTime=true"

	db, err := sql.Open("mysql", dsn)
	if err != nil {
		return nil, err
	}

	if err := db.Ping(); err != nil {
		return nil, err
	}

	return db, nil
}
