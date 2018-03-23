using System;
using CabinetData.Base;
using System.Collections.Generic;
using Dapper;
using System.Linq;
using CabinetData.Entities.QueryEntities;
using DapperExtensions;

namespace CabinetData.Entities
{
	/// <summary>
	/// User custom methods for Cabinet
	/// </summary>
	partial class Cabinet
	{
        public static void UpdateOnLine(List<int> ids)
        {
            using (var cn = Database.GetDbConnection())
            {
                  cn.Execute("update Cabinet set IsOnline=1,LastOnlineTime=getdate() where ID in @ID",new { ID=ids});
            }
        }

        public static void UpdateOffLine(List<int> ids)
        {
            using (var cn = Database.GetDbConnection())
            {
                cn.Execute("update Cabinet set IsOnline=0 where ID in @ID", new { ID = ids });
            }
        }

        public static List<Cabinet> GetAll()
        {
            var sql = "select * from Cabinet";
            using (var cn = Database.GetDbConnection())
            { 
               return cn.Query<Cabinet>(sql).ToList();
            }
        }
        public static List<Cabinet> GetCabinetByIds(List<int> ids)
        {
            var sql = "select * from Cabinet where ID in @ID";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Cabinet>(sql,new { ID=ids}).ToList();
            }
        }

        public static Cabinet GetOne(int id)
        {
            var sql = "select * from Cabinet where ID=@ID";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Cabinet>(sql,new { ID=id}).FirstOrDefault();
            }
        }
        public static Cabinet GetByMac(string mac)
        {
            var sql = "select * from Cabinet where AndroidMac=@Mac";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Cabinet>(sql, new { Mac = mac }).FirstOrDefault();
            }
        }
        public static List<Cabinet> GetByLikeName(string name)
        {
            var sql = "select * from Cabinet where Name like '%" + name + "%'";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Cabinet>(sql).ToList();
            }
        }
        public static Cabinet GetByName(string name)
        {
            var sql = "select * from Cabinet where Name=@Name";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Cabinet>(sql, new { Name = name }).FirstOrDefault();
            }
        }

        public static Page<Cabinet> GetCabinets(CabinetSearchModel search)
        {
            var sql = "select * from Cabinet where 1=1 ";
            if (!string.IsNullOrEmpty(search.CabinetName))
            {
                sql += " and Name like '%" + search.CabinetName + "%'";
            }
            if (!string.IsNullOrEmpty(search.DepartmentName))
            {
                List<Department> departList = Department.GetAll(search.DepartmentName);
                if (departList.Count == 0)
                {
                    sql += " and DepartmentID in (0)";
                }
                else
                {
                    sql += " and DepartmentID in (" + string.Join(",", departList.Select(m => m.ID).ToList()) + ")";
                }
            }
            if (search.CabinetCode != null)
            {
                sql += " and Code like '%" + search.CabinetCode + "%'";
            }
            using (var cn = Database.GetDbConnection())
            {
                return cn.PagedQuery<Cabinet>(search.PageIndex, search.PageSize, sql, new { });
            }
        }


        public static void Add(Cabinet cabinet)
        {
            using (var cn = Database.GetDbConnection())
            {
                cn.Insert<Cabinet>(cabinet);
            }

        }
        public static bool Update(Cabinet cabinet)
        {
            using (var cn = Database.GetDbConnection())
            {
                return cn.Update<Cabinet>(cabinet);
            }

        }

        public static bool Delete(int id)
        {
            var sql = "delete from  Cabinet where ID=@ID";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Execute(sql, new { ID = id }) > 0;
            }
        }
    }
}
