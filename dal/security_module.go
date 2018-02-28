package dal

import (
	"fmt"
	_ "github.com/denisenkom/go-mssqldb"
	"github.com/jmoiron/sqlx"
)

type security_module struct {
	Id          string `db:"id"`
	Description string `db:"description"`
	Name        string `db:"name"`
	Priority    int32  `db:"priority"`
	Url         string `db:"url"`
	Parent_id   string `db:"parent_id"`
	Sn          string `db:"sn"`
}

func security_module_GetAll() (list []security_module, err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	err = db.Select(&list, "select * from security_module")
	return
}
func security_module_Delete(id int) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	_, err = db.Exec("delete from security_module where ID=" + fmt.Sprint(id))
	return
}
func security_module_Update(item security_module) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "update security_module set Description=:Description,Name=:Name,Priority=:Priority,Url=:Url,Parent_id=:Parent_id,Sn=:Sn where Id=:Id"
	_, err = db.Exec(sql, item.Description, item.Name, item.Priority, item.Url, item.Parent_id, item.Sn, item.Id)
	return
}
func security_module_Insert(item security_module) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "insert into security_module (Id,Description,Name,Priority,Url,Parent_id,Sn) values (:Id,:Description,:Name,:Priority,:Url,:Parent_id,:Sn)"
	_, err = db.Exec(sql, item.Id, item.Description, item.Name, item.Priority, item.Url, item.Parent_id, item.Sn)
	return
}
