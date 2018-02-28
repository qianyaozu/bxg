package dal

import (
	_ "github.com/denisenkom/go-mssqldb"
	"github.com/jmoiron/sqlx"
	"time"
)

var MsSqlConnectionString = "sqlserver://sa:jeqee001@127.0.0.1:1433?database=Cabinet"

type CabinetInfo struct {
	ID                string    `db:"ID"`             //保密柜识别码，每个电子密码锁都有一个唯一的ID
	CabinetName       string    `db:"CabinetName"`    //保密柜名称
	CabinetAddress    string    `db:"CabinetAddress"` //保密柜存放地址
	DepartmentID      string    `db:"DepartmentID"`   //所属部门
	DepartmentIDM     string    `db:"DepartmentIDM"`
	CabinetCode       string    `db:"CabinetCode"`      //保密柜用户自定义编号，便于管理用
	CabinetIP         string    `db:"CabinetIP"`        //保密柜分配的IP地址
	CabinetOnline     int32     `db:"CabinetOnline"`    //是否在线标志
	LastOnlineTime    time.Time `db:"LastOnlineTime"`   //最后上线时间
	CabinetStatus     int32     `db:"CabinetStatus"`    //保密柜状态
	CabinetContact    string    `db:"CabinetContact"`   //保密柜指定负责人
	CabinetTelephone  string    `db:"CabinetTelephone"` //保密柜指定负责人联系电话
	CabinetContact1   string    `db:"CabinetContact1"`
	CabinetTelephone1 string    `db:"CabinetTelephone1"`
	NeedOpenConfirm   int32     `db:"needOpenConfirm"`
	LogID             string    `db:"logID"`
	Remark            string    `db:"Remark"` //备注
	CabinetPhone      string    `db:"CabinetPhone"`
	CabinetPhone1     string    `db:"CabinetPhone1"`
	Version           string    `db:"Version"` //android版本号
	CabinetPassword   string    `db:"CabinetPassword"`
}

func CabinetInfo_GetAll() (list []CabinetInfo, err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	err = db.Select(&list, "select * from CabinetInfo")
	return
}
func CabinetInfo_GetOne(id string) (item CabinetInfo, err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	err = db.Get(&item, "select * from CabinetInfo where ID= :ID", id)
	return
}

func CabinetInfo_Delete(id string) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	_, err = db.Exec("delete from CabinetInfo where ID= :ID", id)
	return
}
func CabinetInfo_Update(item CabinetInfo) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "update CabinetInfo set CabinetName=:CabinetName,CabinetAddress=:CabinetAddress,DepartmentID=:DepartmentID,DepartmentIDM=:DepartmentIDM,CabinetCode=:CabinetCode,CabinetIP=:CabinetIP,CabinetOnline=:CabinetOnline,LastOnlineTime=:LastOnlineTime,CabinetStatus=:CabinetStatus,CabinetContact=:CabinetContact,CabinetTelephone=:CabinetTelephone,CabinetContact1=:CabinetContact1,CabinetTelephone1=:CabinetTelephone1,NeedOpenConfirm=:NeedOpenConfirm,LogID=:LogID,Remark=:Remark,CabinetPhone=:CabinetPhone,CabinetPhone1=:CabinetPhone1,Version=:Version,CabinetPassword=:CabinetPassword where ID=:ID"
	_, err = db.Exec(sql, item.CabinetName, item.CabinetAddress, item.DepartmentID, item.DepartmentIDM, item.CabinetCode, item.CabinetIP, item.CabinetOnline, item.LastOnlineTime, item.CabinetStatus, item.CabinetContact, item.CabinetTelephone, item.CabinetContact1, item.CabinetTelephone1, item.NeedOpenConfirm, item.LogID, item.Remark, item.CabinetPhone, item.CabinetPhone1, item.Version, item.CabinetPassword, item.ID)
	return
}
func CabinetInfo_Insert(item CabinetInfo) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "insert into CabinetInfo (ID,CabinetName,CabinetAddress,DepartmentID,DepartmentIDM,CabinetCode,CabinetIP,CabinetOnline,LastOnlineTime,CabinetStatus,CabinetContact,CabinetTelephone,CabinetContact1,CabinetTelephone1,NeedOpenConfirm,LogID,Remark,CabinetPhone,CabinetPhone1,Version,CabinetPassword) values (:ID,:CabinetName,:CabinetAddress,:DepartmentID,:DepartmentIDM,:CabinetCode,:CabinetIP,:CabinetOnline,:LastOnlineTime,:CabinetStatus,:CabinetContact,:CabinetTelephone,:CabinetContact1,:CabinetTelephone1,:NeedOpenConfirm,:LogID,:Remark,:CabinetPhone,:CabinetPhone1,:Version,:CabinetPassword)"
	_, err = db.Exec(sql, item.ID, item.CabinetName, item.CabinetAddress, item.DepartmentID, item.DepartmentIDM, item.CabinetCode, item.CabinetIP, item.CabinetOnline, item.LastOnlineTime, item.CabinetStatus, item.CabinetContact, item.CabinetTelephone, item.CabinetContact1, item.CabinetTelephone1, item.NeedOpenConfirm, item.LogID, item.Remark, item.CabinetPhone, item.CabinetPhone1, item.Version, item.CabinetPassword)
	return
}
