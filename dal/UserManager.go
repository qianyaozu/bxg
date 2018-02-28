package dal

import (
	"fmt"
	_ "github.com/denisenkom/go-mssqldb"
	"github.com/jmoiron/sqlx"
	"time"
)

type UserManager struct {
	ID               string    `db:"ID"`               //自动ID
	ManagerName      string    `db:"ManagerName"`      //管理员名称
	ManagerPassword  string    `db:"ManagerPassword"`  //管理员密码，需要加密
	DepartmentID     string    `db:"DepartmentID"`     //部门ID
	LastLoginIP      string    `db:"LastLoginIP"`      //最后一次登录IP
	LastLoginTime    time.Time `db:"LastLoginTime"`    //最后登录时间
	RealName         string    `db:"RealName"`         //真实姓名
	ManagerTelephone string    `db:"ManagerTelephone"` //管理员联系电话
	ManagerEmail     string    `db:"ManagerEmail"`     //邮箱地址
	Remark           string    `db:"Remark"`           //备注
	CreateTime       time.Time `db:"createTime"`
	Salt             string    `db:"salt"`
	Status           string    `db:"status"`
}

func UserManager_GetAll() (list []UserManager, err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	err = db.Select(&list, "select * from UserManager")
	return
}
func UserManager_Delete(id int) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	_, err = db.Exec("delete from UserManager where ID=" + fmt.Sprint(id))
	return
}
func UserManager_Update(item UserManager) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "update UserManager set ManagerName=:ManagerName,ManagerPassword=:ManagerPassword,DepartmentID=:DepartmentID,LastLoginIP=:LastLoginIP,LastLoginTime=:LastLoginTime,RealName=:RealName,ManagerTelephone=:ManagerTelephone,ManagerEmail=:ManagerEmail,Remark=:Remark,CreateTime=:CreateTime,Salt=:Salt,Status=:Status where ID=:ID"
	_, err = db.Exec(sql, item.ManagerName, item.ManagerPassword, item.DepartmentID, item.LastLoginIP, item.LastLoginTime, item.RealName, item.ManagerTelephone, item.ManagerEmail, item.Remark, item.CreateTime, item.Salt, item.Status, item.ID)
	return
}
func UserManager_Insert(item UserManager) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "insert into UserManager (ID,ManagerName,ManagerPassword,DepartmentID,LastLoginIP,LastLoginTime,RealName,ManagerTelephone,ManagerEmail,Remark,CreateTime,Salt,Status) values (:ID,:ManagerName,:ManagerPassword,:DepartmentID,:LastLoginIP,:LastLoginTime,:RealName,:ManagerTelephone,:ManagerEmail,:Remark,:CreateTime,:Salt,:Status)"
	_, err = db.Exec(sql, item.ID, item.ManagerName, item.ManagerPassword, item.DepartmentID, item.LastLoginIP, item.LastLoginTime, item.RealName, item.ManagerTelephone, item.ManagerEmail, item.Remark, item.CreateTime, item.Salt, item.Status)
	return
}
