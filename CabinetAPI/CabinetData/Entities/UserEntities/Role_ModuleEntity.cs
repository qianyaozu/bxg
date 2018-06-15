using System;

using CabinetData.Base;
using System.Collections.Generic;
using Dapper;
using System.Linq;
using DapperExtensions;

namespace CabinetData.Entities
{
    /// <summary>
    /// User custom methods for Role_Module
    /// </summary>
    partial class Role_Module
    {
        public static List<Role_Module> Get(int roleID)
        {
            var sql = "select * from Role_Module  where RoleID=@RoleID";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Role_Module>(sql, new { RoleID = roleID }).ToList();
            }



        }


        public static List<Role_Module> GetIds(List<int> ids)
        {
            var sql = "select * from Role_Module  where ID in @ID";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Role_Module>(sql, new { ID = ids }).ToList();
            }
        }


        public static bool UpdateRoleModule(Role_Module role)
        {
            using (var cn = Database.GetDbConnection())
            {
                return cn.Update<Role_Module>(role);
            }
        }

        public static void Insert(List<Role_Module> roles)
        { 
            using (var cn = Database.GetDbConnection())
            {
                 cn.Insert<Role_Module>(roles);
            }
        }
    }
}
