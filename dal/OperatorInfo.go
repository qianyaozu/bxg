package dal

import (
	"fmt"
	_ "github.com/denisenkom/go-mssqldb"
	"github.com/jmoiron/sqlx"
)

type OperatorInfo struct {
	ID               string `db:"ID"`           //操作员ID，可以使用工号
	OperatorName     string `db:"OperatorName"` //操作员名称
	DepartmentID     string `db:"DepartmentID"` //所属部门ID
	OperatorTelehone string `db:"OperatorTelehone"`
	OperatorEmail    string `db:"OperatorEmail"`
	Remark           string `db:"Remark"`
}

func OperatorInfo_GetAll() (list []OperatorInfo, err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	err = db.Select(&list, "select * from OperatorInfo")
	return
}
func OperatorInfo_Delete(id int) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	_, err = db.Exec("delete from OperatorInfo where ID=" + fmt.Sprint(id))
	return
}
func OperatorInfo_Update(item OperatorInfo) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "update OperatorInfo set OperatorName=:OperatorName,DepartmentID=:DepartmentID,OperatorTelehone=:OperatorTelehone,OperatorEmail=:OperatorEmail,Remark=:Remark where ID=:ID"
	_, err = db.Exec(sql, item.OperatorName, item.DepartmentID, item.OperatorTelehone, item.OperatorEmail, item.Remark, item.ID)
	return
}
func OperatorInfo_Insert(item OperatorInfo) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "insert into OperatorInfo (ID,OperatorName,DepartmentID,OperatorTelehone,OperatorEmail,Remark) values (:ID,:OperatorName,:DepartmentID,:OperatorTelehone,:OperatorEmail,:Remark)"
	_, err = db.Exec(sql, item.ID, item.OperatorName, item.DepartmentID, item.OperatorTelehone, item.OperatorEmail, item.Remark)
	return
}
