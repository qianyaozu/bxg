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
                return cn.Query<CabinetLog>(sql,new { ID =cabinetID}).ToList();
            }
        }

        public static CabinetLog GetOpenLog(int cabinetID)
        {
            var sql = "select  top 1 * from CabinetLog  where CabinetID=@ID and OperationType in(1,4) order by id desc";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<CabinetLog>(sql, new { ID = cabinetID }).FirstOrDefault();
            }
        }


        public static Page<CabinetLogA> GetCabinets(CabinetLogSearchModel search,List<int> departList)
        {
            var sql = "select * from CabinetLog where DepartmentID in @DepartmentID ";
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
                sql += " and OperationType = " + search.OperatorType;
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
                return cn.PagedQuery<CabinetLogA>(search.PageIndex, search.PageSize, sql, new { DepartmentID= departList });
            }
        }


        public static void Add(CabinetLog log)
        {
            if (log.OperationType == (int)Principal.OperatorTypeEnum.心跳)
                return;
            using (var cn = Database.GetDbConnection())
            {
                cn.Insert<CabinetLog>(log);
            }
        }

        public static void Delete(List<int> ids)
        {
            using (var cn = Database.GetDbConnection())
            {
                cn.Execute("delete from CabinetLog where ID in @ID", new { ID = ids });
            }
        }

        public static void Add(List<CabinetLog> logs)
        {
            using (var cn = Database.GetDbConnection())
            {
                cn.Insert<CabinetLog>(logs);
            }
        }



        /// <summary>
        /// 获取部门报警统计
        /// </summary>
        /// <param name="departID"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static List<DepartmentStatistics> DepartAlarmStatistics(int departID, string from, string to)
        {
            using (var cn = Database.GetDbConnection())
            {
                var list = Department.GetAllChildren(departID);
                if (list.Count == 0)
                    return new List<DepartmentStatistics>();
                return cn.Query<DepartmentStatistics>("select DepartmentID,count(1) as Count from Cabinetlog where DepartmentID in @ID and createtime between @Start and @End and OperationType in(2,4,5,6,7,8,9,10) group by DepartmentID", new { ID = list.Select(m => m.ID).ToList(), Start = from, End = to }).ToList();
            }
        }

        /// <summary>
        /// 获取部门每月报警统计
        /// </summary>
        /// <param name="departID"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static List<DepartmentMonthStatistics> DepartMonthAlarmStatistics(int departID, string from, string to)
        {
            using (var cn = Database.GetDbConnection())
            {
                var list = Department.GetAllChildren(departID);
                if (list.Count == 0)
                    return new List<DepartmentMonthStatistics>();
                return cn.Query<DepartmentMonthStatistics>("select Year(CreateTime) as [Year],Month(CreateTime) as [Month],count(1) as Count from Cabinetlog where DepartmentID in @ID and createtime between @Start and @End and OperationType in(2,4,5,6,7,8,9,10) group by Year(CreateTime),Month(CreateTime)", new { ID = list.Select(m => m.ID).ToList(), Start = from, End = to }).ToList();
            }
        }


        /// <summary>
        /// 报警类型统计
        /// </summary>
        /// <param name="departID"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static List<DepartmentAlarmTypeStatistics> DepartAlarmTypeStatistics(int departID, string from, string to)
        {
            using (var cn = Database.GetDbConnection())
            {
                var list = Department.GetAllChildren(departID);
                if (list.Count == 0)
                    return new List<DepartmentAlarmTypeStatistics>();
                return cn.Query<DepartmentAlarmTypeStatistics>("select OperationType,count(1) as Count from Cabinetlog where DepartmentID in @ID and createtime between @Start and @End and OperationType in(2,4,5,6,7,8,9,10) group by OperationType", new { ID = list.Select(m => m.ID).ToList(), Start = from, End = to }).ToList();
            }
        }


        public static List<CabinetLog> GetAllFromStart(int startID)
        {
            var sql = "select top 1000 * from CabinetLog  where ID>@ID";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<CabinetLog>(sql,new
                {
                    ID = startID
                }).ToList();
            }
        }
    }


    public class DepartmentAlarmTypeStatistics
    {
        public int OperationType { get; set; }
        public int Count { get; set; }
    }
    public class DepartmentStatistics
    {
        public string DepartmentName { get; set; }
        public int DepartmentID { get; set; }

        public int Count { get; set; }
    }

    public class DepartmentMonthStatistics
    {  
        public int Year { get; set; }
        public int Month { get; set; }

        public int Count { get; set; }
    }
}
