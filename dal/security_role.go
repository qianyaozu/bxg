package dal

import (
	"fmt"
	_ "github.com/denisenkom/go-mssqldb"
	"github.com/jmoiron/sqlx"
)

type security_role struct {
	Id   string `db:"id"`
	Name string `db:"name"`
}

func security_role_GetAll() (list []security_role, err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	err = db.Select(&list, "select * from security_role")
	return
}
func security_role_Delete(id int) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	_, err = db.Exec("delete from security_role where ID=" + fmt.Sprint(id))
	return
}
func security_role_Update(item security_role) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "update security_role set Name=:Name where Id=:Id"
	_, err = db.Exec(sql, item.Name, item.Id)
	return
}
func security_role_Insert(item security_role) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "insert into security_role (Id,Name) values (:Id,:Name)"
	_, err = db.Exec(sql, item.Id, item.Name)
	return
}
