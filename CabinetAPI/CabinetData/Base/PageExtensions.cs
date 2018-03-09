using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using DapperExtensions;
using CabinetData.Entities.QueryEntities;

namespace CabinetData.Base
{
    /// <summary>
    /// 分页查询的结果。
    /// </summary>
    /// <typeparam name="T"></typeparam>
   

    /// <summary>
    /// 分页查询扩展。
    /// </summary>
    public static class PageExtensions
    {
        static Regex rxColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        static Regex rxOrderBy = new Regex(@"\bORDER\s+BY\s+(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        static Regex rxDistinct = new Regex(@"\ADISTINCT\s", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// 查询一页。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">数据连接对象</param>
        /// <param name="page">当前页，从1开始编号</param>
        /// <param name="itemsPerPage">每页多少记录</param>
        /// <param name="sql">Sql语句（如：Select * from Users Where 1 = 1 Order By Id Asc）</param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static Page<T> PagedQuery<T>(this IDbConnection connection, long page, long itemsPerPage, string sql, dynamic param = null) where T : class
        {
            int rowCount = 0;

            // 分页查询原型

            //var sql = "SELECT COUNT(*) FROM Customer; ";
            //sql += "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY CustomerId ASC) AS Row, * FROM Customer) AS Paged ";
            //var pageStart = (page - 1) * itemsPerPage;
            //sql += string.Format(" WHERE Row > {0} AND Row <={1}", pageStart, (pageStart + itemsPerPage));

            string sqlSelectRemoved, sqlOrderBy;
            string sqlCount;
            if (!SplitSqlForPaging(sql, out sqlCount, out sqlSelectRemoved, out sqlOrderBy))
            {
                return new Page<T> { CurrentPage = 0, TotalItems = 0, Items = new List<T>(), ItemsPerPage = itemsPerPage, TotalPages = 0 };
            }

            sqlSelectRemoved = rxOrderBy.Replace(sqlSelectRemoved, "");

            var pageStart = (page - 1) * itemsPerPage;
            string sqlPage = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) AS Row, {1}) AS Paged ",
                sqlOrderBy == null ? "ORDER BY (SELECT NULL)" : sqlOrderBy,
                sqlSelectRemoved);
            sqlPage += string.Format(" WHERE Row > {0} AND Row <= {1}", pageStart, (pageStart + itemsPerPage));

            sql = sqlCount + ";" + sqlPage;

            var multi = connection.QueryMultiple(sql, (object)param);
            rowCount = multi.Read<int>().Single();
            var users = multi.Read<T>().ToList();

            var result = new Page<T>();
            result.Items = users;
            result.TotalItems = rowCount;
            result.CurrentPage = page;
            result.TotalPages = (rowCount - 1) / itemsPerPage + 1;

            return result;
        }






        public static List<dynamic> PagedQueryByTop<T>(this IDbConnection connection, long page, long itemsPerPage, string sql, int num, dynamic param = null) where T : class
        {
            int rowCount = 0;

            // 分页查询原型

            //var sql = "SELECT COUNT(*) FROM Customer; ";
            //sql += "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY CustomerId ASC) AS Row, * FROM Customer) AS Paged ";
            //var pageStart = (page - 1) * itemsPerPage;
            //sql += string.Format(" WHERE Row > {0} AND Row <={1}", pageStart, (pageStart + itemsPerPage));

            string sqlSelectRemoved, sqlOrderBy;
            string sqlCount;
            if (!SplitSqlForPaging(sql, out sqlCount, out sqlSelectRemoved, out sqlOrderBy))
            {
                return new List<dynamic>();
            }

            sqlSelectRemoved = rxOrderBy.Replace(sqlSelectRemoved, "");

            var pageStart = (page - 1) * itemsPerPage;
            string sqlPage = string.Format("SELECT top {0} * FROM (SELECT ROW_NUMBER() OVER ({1}) AS Row, {2}) AS Paged ", num,
                sqlOrderBy == null ? "ORDER BY (SELECT NULL)" : sqlOrderBy,
                sqlSelectRemoved);
            sqlPage += string.Format(" WHERE Row > {0} AND Row <= {1}", pageStart, (pageStart + itemsPerPage));
            return connection.Query(sqlPage, (object)param).ToList();

        }
        private static bool SplitSqlForPaging(string sql, out string sqlCount, out string sqlSelectRemoved, out string sqlOrderBy)
        {
            sqlSelectRemoved = null;
            sqlCount = null;
            sqlOrderBy = null;

            // Extract the columns from "SELECT <whatever> FROM"
            var m = rxColumns.Match(sql);
            if (!m.Success)
                return false;

            // Save column list and replace with COUNT(*)
            Group g = m.Groups[1];
            sqlSelectRemoved = sql.Substring(g.Index);

            if (rxDistinct.IsMatch(sqlSelectRemoved))
                sqlCount = sql.Substring(0, g.Index) + "COUNT(" + m.Groups[1].ToString().Trim() + ") " + sql.Substring(g.Index + g.Length);
            else
                sqlCount = sql.Substring(0, g.Index) + "COUNT(1) " + sql.Substring(g.Index + g.Length);


            // Look for an "ORDER BY <whatever>" clause
            m = rxOrderBy.Match(sqlCount);
            if (!m.Success)
            {
                sqlOrderBy = null;
            }
            else
            {
                g = m.Groups[0];
                sqlOrderBy = g.ToString();
                sqlCount = sqlCount.Substring(0, g.Index) + sqlCount.Substring(g.Index + g.Length);
            }

            return true;
        }
    }
}
