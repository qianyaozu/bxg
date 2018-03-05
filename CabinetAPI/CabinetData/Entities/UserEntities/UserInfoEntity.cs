using System;

using CabinetData.Base;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using CabinetData.Entities.QueryEntities;
using DapperExtensions;

namespace CabinetData.Entities
{
    /// <summary>
    /// User custom methods for UserInfo
    /// </summary>
    partial class UserInfo
    {
        public static UserInfo GetOne(int id)
        {
            var sql = "select * from UserInfo where ID=@ID";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<UserInfo>(sql, new { ID = id }).FirstOrDefault();
            }
        }
        public static UserInfo GetOne(string name)
        {
            var sql = "select * from UserInfo where Name=@Name";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<UserInfo>(sql, new { Name = name }).FirstOrDefault();
            }
        }

        public static List<UserInfo> GetLikeName(string name)
        {
            var sql = "select * from UserInfo where Name like '%@Name%'";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<UserInfo>(sql, new { Name = name }).ToList();
            }
        }

        public static Page<UserInfo> GetUsers(UserSearchModel search )
        {
            var sql = "select * from UserInfo where 1=1 ";
            if (!string.IsNullOrEmpty(search.UserName))
            {
                sql += " and Name like '%" + search.UserName + "%'";
            }
            if (search.DepartmentID != null)
            {
                sql += " and DepartmentID in (" + string.Join(",", Department.GetChildrenID(new List<int> { search.DepartmentID.Value })) + ")";
            }
            if (search.RoleID != null)
            {
                sql += " and RoleID = " + search.RoleID;
            }
            using (var cn = Database.GetDbConnection())
            {
                return cn.PagedQuery<UserInfo>(search.PageIndex, search.PageSize, sql, new { });
            }
        }


        public static void Add(UserInfo user)
        {
            using (var cn = Database.GetDbConnection())
            {
                cn.Insert<UserInfo>(user);
            }

        }
        public static bool Update(UserInfo user)
        {
            using (var cn = Database.GetDbConnection())
            {
                return cn.Update<UserInfo>(user);
            }

        }

        public static bool Delete(int id)
        {
            var sql = "delete from  UserInfo where ID=@ID";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Execute(sql, new { ID = id }) > 0;
            }
        }
    }
}
