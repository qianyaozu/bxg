using CabinetData.Entities;
using CabinetData.Entities.QueryEntities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CabinetAPI.Controllers
{
    public class DepartmentController : BaseController
    {


        /// <summary>
        /// 获取登录用户的部门树
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("api/department/tree")]
        public IHttpActionResult Tree()
        {
            string userId = GetSession("UserID") ?? "";
            string roleId = GetSession("RoleID") ?? "";
            string departmentId = GetSession("DepartmentID") ?? "";
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleId) || string.IsNullOrEmpty(departmentId))
                return Failure("登录过期,请重新登录");
            try
            {
                List<int> list = Department.GetChildrenID(new List<int> { Int32.Parse(departmentId) });
                if(list.Count==0)
                    return Failure("查询错误");
                return Success(Department.GetAll(list));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("查询错误");
            }
        }



        [HttpPost, Route("api/department/add")]
        public IHttpActionResult AddDepartment(Department depart)
        {
            try
            {
                string valiate = ValiateDepartmentModel(depart);
                if (!string.IsNullOrEmpty(valiate))
                {
                    return Failure(valiate);
                }
                Department.Add(depart);
                return Success(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("新增失败");
            }

        }

        /// <summary>
        /// 验证用户实体类
        /// </summary>
        /// <param name="depart"></param>
        /// <returns></returns>
        private string ValiateDepartmentModel(Department depart)
        {
            if (depart == null)
                return "参数错误"; 
            if (string.IsNullOrEmpty(depart.Name))
                return "请设置部门名称";
            return null;
        }

        [HttpPost, Route("api/department/edit")]
        public IHttpActionResult EditDepartment(Department depart)
        {
            try
            {
                string valiate = ValiateDepartmentModel(depart);
                if (!string.IsNullOrEmpty(valiate))
                {
                    return Failure(valiate);
                }
                if (depart.ID == 0)
                    return Failure("未指定部门");
                var dp = Department.GetOne(depart.ID);
                if (dp == null)
                    return Failure("未找到指定部门");
                dp.Name = depart.Name;
                dp.ParentID = depart.ParentID;
                dp.Remark = depart.Remark;
                dp.SortID = depart.SortID;
                Department.Update(dp);
                return Success(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("修改失败");
            }
        }

        [HttpPost, Route("api/department/delete")]
        public IHttpActionResult DeleteDepartment(int departID)
        {
            if (departID == 0)
            {
                return Failure("未指定部门");
            }
            try
            {
                Department.Delete(departID);
                return Success(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("删除失败");
            }
        }

        [HttpPost, Route("api/department/view")]
        public IHttpActionResult QueryDepartment(DepartmentSearchModel search)
        {
            try
            {
                var result = Department.GetDepartment(search);
                return Success(result);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("查询失败");
            }
        }
    }
}