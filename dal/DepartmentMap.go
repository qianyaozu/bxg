package dal

import (
	"fmt"
	_ "github.com/denisenkom/go-mssqldb"
	"github.com/jmoiron/sqlx"
)

type DepartmentMap struct {
	Id        int32  `db:"id"`
	Unit_id   string `db:"unit_id"`
	Map_size  string `db:"map_size"`
	PositionX string `db:"positionX"`
	PositionY string `db:"positionY"`
}

func DepartmentMap_GetAll() (list []DepartmentMap, err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	err = db.Select(&list, "select * from DepartmentMap")
	return
}
func DepartmentMap_Delete(id int) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}
	defer db.Close()
	_, err = db.Exec("delete from DepartmentMap where ID=" + fmt.Sprint(id))
	return
}
func DepartmentMap_Update(item DepartmentMap) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "update DepartmentMap set Unit_id=:Unit_id,Map_size=:Map_size,PositionX=:PositionX,PositionY=:PositionY where Id=:Id"
	_, err = db.Exec(sql, item.Unit_id, item.Map_size, item.PositionX, item.PositionY, item.Id)
	return
}
func DepartmentMap_Insert(item DepartmentMap) (err error) {
	db, err := sqlx.Connect("mssql", MsSqlConnectionString)
	if err != nil {
		return
	}

	defer db.Close()
	var sql = "insert into DepartmentMap (Unit_id,Map_size,PositionX,PositionY) values (:Unit_id,:Map_size,:PositionX,:PositionY)"
	_, err = db.Exec(sql, item.Unit_id, item.Map_size, item.PositionX, item.PositionY)
	return
}
