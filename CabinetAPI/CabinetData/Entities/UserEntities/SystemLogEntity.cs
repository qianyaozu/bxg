using System;

using CabinetData.Base;
using CabinetData.Entities.QueryEntities;
using DapperExtensions;
using System.Collections.Generic;

namespace CabinetData.Entities
{
	/// <summary>
	/// User custom methods for SystemLog
	/// </summary>
	partial class SystemLog
	{
        public static Page<SystemLog> GetSystemLogs(SystemLogSearchModel search,List<int> departList)
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
                return cn.PagedQuery<SystemLog>(search.PageIndex, search.PageSize, sql, new { DepartmentID = departList });
            }
        }


        public static void Add(SystemLog log)
        {
            using (var cn = Database.GetDbConnection())
            {
                cn.Insert<SystemLog>(log);
            }
        }


        
    }
}
