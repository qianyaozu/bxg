package dal

import (
	"fmt"
	_ "github.com/denisenkom/go-mssqldb"
	"github.com/jmoiron/sqlx"
)

type security_role_permission struct {
	Role_id    string `db:"role_id"`
	Permission string `db:"permission"`
}

func security_role_permission_GetAll() (list []security_role_permission, err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	err = db.Select(&list, "select * from security_role_permission")
	return
}
func security_role_permission_Insert(item security_role_permission) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "insert into security_role_permission (Role_id,Permission) values (:Role_id,:Permission)"
	_, err = db.Exec(sql, item.Role_id, item.Permission)
	return
}
