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
	/// User custom methods for CabinetLog
	/// </summary>
	partial class CabinetLog
	{
        public static List<CabinetLog> GetAll(int cabinetID)
        {
            var sql = "select * from CabinetLog  where CabinetID=@ID";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<CabinetLog>(sql).ToList();
            }
        }


        public static Page<CabinetLog> GetCabinets(CabinetLogSearchModel search)
        {
            var sql = "select * from CabinetLog where 1=1 ";
            if ((search.CabinetID ?? 0) > 0)
            {
                sql += " and CabinetID = " + search.CabinetID;
            }
            if (!string.IsNullOrEmpty(search.CabinetName))
            {
                List<Cabinet> cabinets = Cabinet.GetByLikeName(search.CabinetName);
                if (cabinets.Count == 0)
                    sql += " and 1=2 ";
                else
                {
                    sql += " and CabinetID in (" + string.Join(",", cabinets.Select(m => m.ID).ToList()) + ")";
                }
            }
            if (!string.IsNullOrEmpty(search.OperatorType))
            {
                sql += " and OperatorType = '" + search.OperatorType + "";
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
                return cn.PagedQuery<CabinetLog>(search.PageIndex, search.PageSize, sql, new { });
            }
        }


        public static void Add(CabinetLog log)
        {
            if (log.OperationType == (int)Principal.OperatorTypeEnum.ÐÄÌø)
                return;
            using (var cn = Database.GetDbConnection())
            {
                cn.Insert<CabinetLog>(log);
            }
        }
        public static void Add(List<CabinetLog> logs)
        {
            using (var cn = Database.GetDbConnection())
            {
                cn.Insert<CabinetLog>(logs);
            }
        }
    }
}
