package dal

import (
	"fmt"
	_ "github.com/denisenkom/go-mssqldb"
	"github.com/jmoiron/sqlx"
)

type security_user_role struct {
	Id       string `db:"id"`
	Priority int32  `db:"priority"`
	Role_id  string `db:"role_id"`
	User_id  string `db:"user_id"`
}

func security_user_role_GetAll() (list []security_user_role, err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	err = db.Select(&list, "select * from security_user_role")
	return
}
func security_user_role_Delete(id int) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	_, err = db.Exec("delete from security_user_role where ID=" + fmt.Sprint(id))
	return
}
func security_user_role_Update(item security_user_role) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "update security_user_role set Priority=:Priority,Role_id=:Role_id,User_id=:User_id where Id=:Id"
	_, err = db.Exec(sql, item.Priority, item.Role_id, item.User_id, item.Id)
	return
}
func security_user_role_Insert(item security_user_role) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "insert into security_user_role (Id,Priority,Role_id,User_id) values (:Id,:Priority,:Role_id,:User_id)"
	_, err = db.Exec(sql, item.Id, item.Priority, item.Role_id, item.User_id)
	return
}
