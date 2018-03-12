using CabinetAPI.Filter;
using CabinetData.Entities;
using CabinetData.Entities.QueryEntities;
using CabinetUtility;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CabinetAPI.Controllers
{
    /// <summary>
    /// 部门信息
    /// </summary>
    [TokenFilter]
    public class DepartmentController : BaseController
    {
        /// <summary>
        /// 获取登录用户的部门树(登陆用户的下级部门)
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("api/department/tree")]
        public IHttpActionResult Tree()
        { 
            try
            {
                UserInfo userCookie = CacheHelper.GetCache(GetCookie("token")) as UserInfo;
                if (userCookie == null)
                {
                    return Failure("登录失效");
                }
                List<int> list = Department.GetChildrenID(new List<int> { userCookie.DepartmentID });
                if (list.Count == 0)
                    return Failure("查询错误");
                return Success(Department.GetAll(list));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("查询错误");
            }
        }

        /// <summary>
        /// 新增部门
        /// </summary>
        /// <param name="depart"></param>
        /// <returns></returns>

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
                
                if (Department.GetOne(depart.Name) != null)
                    return Failure("该部门已经存在");


                UserInfo userCookie = CacheHelper.GetCache(GetCookie("token")) as UserInfo;
                if (userCookie == null)
                {
                    return Failure("登录失效");
                }
                SystemLog.Add(new SystemLog
                {
                    Action = "AddDepartment",
                    LogContent = userCookie.Name + "-新增部门-" + depart.Name,
                    CreateTime = DateTime.Now,
                    UserID = userCookie.ID,
                    RoleID = userCookie.RoleID,
                    DepartmentID = userCookie.DepartmentID,
                    ClientIP = GetIP(),
                    UserName = userCookie.Name,
                    RealName = userCookie.RealName
                });
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
        /// <summary>
        /// 修改部门
        /// </summary>
        /// <param name="depart"></param>
        /// <returns></returns>
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

                var old = Department.GetOne(depart.Name);
                if (old != null && old.ID != depart.ID)
                    return Failure("该部门名称已经被使用");


                UserInfo userCookie = CacheHelper.GetCache(GetCookie("token")) as UserInfo;
                if (userCookie == null)
                {
                    return Failure("登录失效");
                }
                SystemLog.Add(new SystemLog
                {
                    Action = "EditDepartment",
                    LogContent = userCookie.Name + "-编辑部门-" + depart.ID,
                    CreateTime = DateTime.Now,
                    UserID = userCookie.ID,
                    RoleID = userCookie.RoleID,
                    DepartmentID = userCookie.DepartmentID,
                    ClientIP = GetIP(),
                    UserName = userCookie.Name,
                    RealName = userCookie.RealName
                });
                dp.CenterIP = depart.CenterIP;
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

        /// <summary>
        /// 删除部门
        /// </summary>
        /// <param name="departID"></param>
        /// <returns></returns>
        [HttpPost, Route("api/department/delete")]
        public IHttpActionResult DeleteDepartment(int departID)
        {
            if (departID == 0)
            {
                return Failure("未指定部门");
            }
            try
            {
                UserInfo userCookie = CacheHelper.GetCache(GetCookie("token")) as UserInfo;
                if (userCookie == null)
                {
                    return Failure("登录失效");
                }
                SystemLog.Add(new SystemLog
                {
                    Action = "DeleteDepartment",
                    LogContent = userCookie.Name + "-删除部门-" + departID,
                    CreateTime = DateTime.Now,
                    UserID = userCookie.ID,
                    RoleID = userCookie.RoleID,
                    DepartmentID = userCookie.DepartmentID,
                    ClientIP = GetIP(),
                    UserName = userCookie.Name,
                    RealName = userCookie.RealName
                });

                Department.Delete(departID);
                return Success(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("删除失败");
            }
        }

        /// <summary>
        /// 分页查询部门信息
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
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