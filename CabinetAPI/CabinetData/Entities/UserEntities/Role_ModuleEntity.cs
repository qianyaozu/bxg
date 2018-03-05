using System;

using CabinetData.Base;
using System.Collections.Generic;
using Dapper;
using System.Linq;

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
    }
}
