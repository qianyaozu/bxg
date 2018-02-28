package dal

import (
	"fmt"
	_ "github.com/denisenkom/go-mssqldb"
	"github.com/jmoiron/sqlx"
)

type DepartmentInfo struct {
	ID                 string `db:"ID"` //部门唯一ID
	Remark             string `db:"Remark"`
	DepartmentName     string `db:"DepartmentName"` //部门名称
	NextCenterIP       string `db:"nextCenterIP"`
	BelongDepartmentID string `db:"BelongDepartmentID"` //直接上级部门ID
	DepartmentRank     int32  `db:"DepartmentRank"`     //部门级别
	Priority           int32  `db:"priority"`
	MapAddress         string `db:"mapAddress"`
}

func DepartmentInfo_GetAll() (list []DepartmentInfo, err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	err = db.Select(&list, "select * from DepartmentInfo")
	return
}
func DepartmentInfo_Delete(id int) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	_, err = db.Exec("delete from DepartmentInfo where ID=" + fmt.Sprint(id))
	return
}
func DepartmentInfo_Update(item DepartmentInfo) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "update DepartmentInfo set Remark=:Remark,DepartmentName=:DepartmentName,NextCenterIP=:NextCenterIP,BelongDepartmentID=:BelongDepartmentID,DepartmentRank=:DepartmentRank,Priority=:Priority,MapAddress=:MapAddress where ID=:ID"
	_, err = db.Exec(sql, item.Remark, item.DepartmentName, item.NextCenterIP, item.BelongDepartmentID, item.DepartmentRank, item.Priority, item.MapAddress, item.ID)
	return
}
func DepartmentInfo_Insert(item DepartmentInfo) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "insert into DepartmentInfo (ID,Remark,DepartmentName,NextCenterIP,BelongDepartmentID,DepartmentRank,Priority,MapAddress) values (:ID,:Remark,:DepartmentName,:NextCenterIP,:BelongDepartmentID,:DepartmentRank,:Priority,:MapAddress)"
	_, err = db.Exec(sql, item.ID, item.Remark, item.DepartmentName, item.NextCenterIP, item.BelongDepartmentID, item.DepartmentRank, item.Priority, item.MapAddress)
	return
}
