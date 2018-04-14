using System;

using CabinetData.Base;
using CabinetData.Entities.QueryEntities;
using System.Collections.Generic;
using Dapper;
using System.Linq;
using DapperExtensions;

namespace CabinetData.Entities
{
    /// <summary>
    /// User custom methods for Department
    /// </summary>
    partial class Department
    {
        public static List<Department> GetAll()
        {
            var sql = "select * from Department ";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Department>(sql).ToList();
            }
        }


        public static List<Department> GetDepartmentByIds(List<int> ids)
        {
            var sql = "select * from Department where ID in @ID ";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Department>(sql, new { ID = ids }).ToList();
            }
        }

        public static List<Department> GetAll(string name)
        {
            var sql = "select * from Department where Name Like '%" + name + "%'";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Department>(sql).ToList();
            }
        }
        public static List<Department> GetAll(List<int> ids)
        {
            var sql = "select * from Department where ID in @ID";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Department>(sql, new { ID = ids }).ToList();
            }
        }
        public static Department GetOne(int id)
        {
            var sql = "select * from Department where ID=@ID ";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Department>(sql, new { ID = id }).FirstOrDefault();
            }
        }
        public static Department GetOne(string name)
        {
            var sql = "select * from Department where Name=@Name ";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Department>(sql, new { Name = name }).FirstOrDefault();
            }
        }

        public static bool Delete(int id)
        {
            var sql = "delete from Department where ID=@ID ";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Execute(sql, new { ID = id }) > 0;
            }
        }

        public static List<Department> GetChildren(int id)
        {
            var sql = "select * from Department where ParentID=@ID ";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Department>(sql, new { ID = id }).ToList();
            }
        }

        public static List<Department> GetAllChildren(int id)
        {
            List<int> idList = GetChildrenID(new List<int> { id });
            var sql = "select * from Department where ID in @ID ";
            using (var cn = Database.GetDbConnection())
            {
                return cn.Query<Department>(sql, new { ID = idList }).ToList();
            }
        }
        public static void Add(Department department)
        {
            using (var cn = Database.GetDbConnection())
            {
                cn.Insert<Department>(department);
            }
        }
        public static bool Update(Department department)
        {
            using (var cn = Database.GetDbConnection())
            {
                return cn.Update<Department>(department);
            }
        }

        public static Page<DepartmentA> GetDepartment(DepartmentSearchModel search,List<int> depart)
        {

            var sql = "select * from Department where ID in @ID "; 
            List<int> IDList = new List<int>();
            if ((search.ParentID ?? 0) != 0)
            {
                IDList = GetChildrenID(new List<int> { search.ParentID.Value });
                sql += " and ParentID in @IDs";
            }

            using (var cn = Database.GetDbConnection())
            {
                return cn.PagedQuery<DepartmentA>(search.PageIndex, search.PageSize, sql, new { IDs = IDList, ID = depart });
            }
        }

        /// <summary>
        /// 递归获取子部门
        /// </summary>
        /// <param name="parentid"></param>
        /// <returns></returns>
        private static List<int> GetChildrenID(List<int> parentid)
        {
            List<int> resultList = new List<int>();
            resultList.AddRange(parentid);
            using (var cn = Database.GetDbConnection())
            {
                var sql = "select id from Department where ParentID in @ParentID";
                List<int> idList = cn.Query<int>(sql, new { ParentID = parentid }).ToList();
                if (idList.Count > 0)
                {
                    resultList.AddRange(GetChildrenID(idList));
                    return resultList;
                }
            }
            return resultList.Distinct().ToList();
        }
    }
}
