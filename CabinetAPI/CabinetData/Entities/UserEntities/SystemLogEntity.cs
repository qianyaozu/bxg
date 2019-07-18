using System;

using CabinetData.Base;
using CabinetData.Entities.QueryEntities;
using DapperExtensions;
using System.Collections.Generic;
using Dapper;
using System.Linq;

namespace CabinetData.Entities
{
	/// <summary>
	/// User custom methods for SystemLog
	/// </summary>
	partial class SystemLog
	{
        public static Page<SystemLogA> GetSystemLogs(SystemLogSearchModel search,List<int> departList)
        {
            var sql = "select * from SystemLog where DepartmentID in @DepartmentID";

            if (!string.IsNullOrEmpty(search.UserName))
            {
                sql += " and UserName like '%" + search.UserName + "%'";
            }

            if (!string.IsNullOrEmpty(search.StartTime))
            {
                sql += " and CreateTime >= '" + search.StartTime + "'";
            }
            if (!string.IsNullOrEmpty(search.EndTime))
            {
                sql += " and CreateTime <= '" + search.EndTime + "'";
            }
            sql += " order by CreateTime desc";
            using (var cn = Database.GetDbConnection())
            {
                return cn.PagedQuery<SystemLogA>(search.PageIndex, search.PageSize, sql, new { DepartmentID = departList });
            }
        }


        public static void Add(SystemLog log)
        {
            using (var cn = Database.GetDbConnection())
            {
                cn.Insert<SystemLog>(log);
            }
        }

        public static List<SystemLog> GetAll(int id) {

            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<SystemLog>("select top 100 * from SystemLog where ID >@ID order by ID", new { ID = id }).ToList<SystemLog>();
            }
        }



    }
}
