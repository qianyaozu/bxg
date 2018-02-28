package dal

import (
	"fmt"
	_ "github.com/denisenkom/go-mssqldb"
	"github.com/jmoiron/sqlx"
	"time"
)

type sys_Log struct {
	Id         int32     `db:"id"`
	Title      string    `db:"Title"`
	LogContent string    `db:"LogContent"`
	CreateTime time.Time `db:"CreateTime"`
	UserName   string    `db:"UserName"`
	RealName   string    `db:"RealName"`
	BranchID   string    `db:"BranchID"`
	BranchName string    `db:"BranchName"`
	ClientIp   string    `db:"ClientIp"`
}

func sys_Log_GetAll() (list []sys_Log, err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	err = db.Select(&list, "select * from sys_Log")
	return
}
func sys_Log_Delete(id int) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	_, err = db.Exec("delete from sys_Log where ID=" + fmt.Sprint(id))
	return
}
func sys_Log_Update(item sys_Log) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "update sys_Log set Title=:Title,LogContent=:LogContent,CreateTime=:CreateTime,UserName=:UserName,RealName=:RealName,BranchID=:BranchID,BranchName=:BranchName,ClientIp=:ClientIp where Id=:Id"
	_, err = db.Exec(sql, item.Title, item.LogContent, item.CreateTime, item.UserName, item.RealName, item.BranchID, item.BranchName, item.ClientIp, item.Id)
	return
}
func sys_Log_Insert(item sys_Log) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "insert into sys_Log (Title,LogContent,CreateTime,UserName,RealName,BranchID,BranchName,ClientIp) values (:Title,:LogContent,:CreateTime,:UserName,:RealName,:BranchID,:BranchName,:ClientIp)"
	_, err = db.Exec(sql, item.Title, item.LogContent, item.CreateTime, item.UserName, item.RealName, item.BranchID, item.BranchName, item.ClientIp)
	return
}
