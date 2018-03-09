using System;

using CabinetData.Base;
using CabinetData.Entities.QueryEntities;
using DapperExtensions;

namespace CabinetData.Entities
{
	/// <summary>
	/// User custom methods for SystemLog
	/// </summary>
	partial class SystemLog
	{
        public static Page<SystemLog> GetSystemLogs(SystemLogSearchModel search)
        {
            var sql = "select * from SystemLog where 1=1 ";

            if (!string.IsNullOrEmpty(search.UserName))
            {
                sql += " and UserName like '%" + search.UserName + "%'";
            }

            if (!string.IsNullOrEmpty(search.StartTime))
            {
                sql += " and CreateTime >= '" + search.StartTime + "";
            }
            if (!string.IsNullOrEmpty(search.EndTime))
            {
                sql += " and CreateTime <= '" + search.EndTime + "";
            }
            using (var cn = Database.GetDbConnection())
            {
                return cn.PagedQuery<SystemLog>(search.PageIndex, search.PageSize, sql, new { });
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
