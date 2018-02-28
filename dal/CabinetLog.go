package dal

import (
	"fmt"
	_ "github.com/denisenkom/go-mssqldb"
	"github.com/jmoiron/sqlx"
	"time"
)

type CabinetLog struct {
	ID             string    `db:"ID"`             //自增ID
	CabinetID      string    `db:"CabinetID"`      //保密柜识别码
	OperatorName   string    `db:"operatorName"`   //操作员ID
	OperateTime    time.Time `db:"OperateTime"`    //操作时间
	OperationType  int32     `db:"OperationType"`  //操作类型
	CabinetIP      string    `db:"CabinetIP"`      //上传数据的IP
	EventContent   string    `db:"EventContent"`   //发生的事件内容
	ServerRecvTime time.Time `db:"ServerRecvTime"` //服务器接收到数据时间
	Remark         string    `db:"Remark"`
	DealPerson     string    `db:"dealPerson"`
	DealTime       time.Time `db:"dealTime"`
	DealResult     string    `db:"dealResult"`
}

func CabinetLog_GetAll() (list []CabinetLog, err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	err = db.Select(&list, "select top 100 * from CabinetLog")
	return
}
func CabinetLog_Delete(id string) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	_, err = db.Exec("delete from CabinetLog where ID= :ID", id)
	return
}
func CabinetLog_Update(item CabinetLog) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "update CabinetLog set CabinetID=:CabinetID,OperatorName=:OperatorName,OperateTime=:OperateTime,OperationType=:OperationType,CabinetIP=:CabinetIP,EventContent=:EventContent,ServerRecvTime=:ServerRecvTime,Remark=:Remark,DealPerson=:DealPerson,DealTime=:DealTime,DealResult=:DealResult where ID=:ID"
	_, err = db.Exec(sql, item.CabinetID, item.OperatorName, item.OperateTime, item.OperationType, item.CabinetIP, item.EventContent, item.ServerRecvTime, item.Remark, item.DealPerson, item.DealTime, item.DealResult, item.ID)
	return
}
func CabinetLog_Insert(item CabinetLog) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "insert into CabinetLog (ID,CabinetID,OperatorName,OperateTime,OperationType,CabinetIP,EventContent,ServerRecvTime,Remark,DealPerson,DealTime,DealResult) values (:ID,:CabinetID,:OperatorName,:OperateTime,:OperationType,:CabinetIP,:EventContent,:ServerRecvTime,:Remark,:DealPerson,:DealTime,:DealResult)"
	_, err = db.Exec(sql, item.ID, item.CabinetID, item.OperatorName, item.OperateTime, item.OperationType, item.CabinetIP, item.EventContent, item.ServerRecvTime, item.Remark, item.DealPerson, item.DealTime, item.DealResult)
	return
}
