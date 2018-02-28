using System;
using CabinetData.Base;
using System.Collections.Generic;
using Dapper;
using System.Linq;

namespace CabinetData.Entities
{
	/// <summary>
	/// User custom methods for Cabinet
	/// </summary>
	partial class Cabinet
	{
        public static List<Cabinet> GetAll()
        {
            var sql = "select * from Cabinet";
            using (var cn = Database.GetDbConnection())
            { 
               return cn.Query<Cabinet>(sql).ToList();
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
        public static Cabinet GetOne(string mac)
        {
            var sql = "select * from Cabinet where AndroidMac=@Mac";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Cabinet>(sql, new { Mac = mac }).FirstOrDefault();
            }
        }
    }
}
