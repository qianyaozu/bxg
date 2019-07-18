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
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                {
                    return Logout();
                }
                
                List<Department> departList = Department.GetAll();
                Department root = departList.Find(m => m.ID == userCookie.DepartmentID);
              
                //List<int> list = Department.GetChildrenID(new List<int> { userCookie.DepartmentID });
                //if (list.Count == 0)
                //    return Failure("查询错误");
                //List<Department> listDepart = Department.GetAll(list);
                //DepartmentTree tree = new DepartmentTree();
                //tree.label = listDepart.Find(m => m.ID == userCookie.DepartmentID).Name;
                //tree.children=

                //List<DepartmentTree> tree = new List<DepartmentTree>();
                //listDepart.ForEach(m =>
                //{
                //    tree.Add(new DepartmentTree
                //    {
                //        ID = m.ID,
                //        ParentID = m.ParentID,
                //        Name = m.Name,
                //        label = m.Name,
                //        Address = m.Address,
                //        SortID = m.SortID,
                //        Remark = m.Remark,
                //        CenterIP = m.CenterIP
                //    });
                //});
                return Success(FindTree(root, departList));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("查询错误");
            }
        }


        

        /// <summary>
        /// 获取登陆用户的下级部门
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("api/department/lowerdepart")]
        public IHttpActionResult LowerDepartment()
        {
            try
            {
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                {
                    return Logout();
                }

                List<Department> departList = Department.GetAllChildren(userCookie.DepartmentID);
                 
                return Success(departList.Select(m => new DepartmentLower { label = m.Name, value = m.ID }).ToList());
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("查询错误");
            }
        }


        private DepartmentTree FindTree(Department root, List<Department> departList)
        {
            DepartmentTree tree = new DepartmentTree();
            tree.ID = root.ID;
            tree.ParentName = departList.Find(m => m.ID == root.ParentID)?.Name;
            tree.label = root.Name;
            tree.children = new List<DepartmentTree>();
            departList.FindAll(m => m.ParentID == root.ID).ForEach(m =>
                {
                    tree.children.Add(FindTree(m, departList));
                });
            return tree;
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


                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                {
                    return Logout();
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
                if (depart.Name=="根部门")
                    return Failure("根部门不能修改");
                if (depart.ID == 0)
                    return Failure("未指定部门");
                var dp = Department.GetOne(depart.ID);
                if (dp == null)
                    return Failure("未找到指定部门");
               
                var old = Department.GetOne(depart.Name);
                if (old != null && old.ID != depart.ID)
                    return Failure("该部门名称已经被使用");


                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                {
                    return Logout();
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
                dp.Address = depart.Address;
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
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                {
                    return Logout();
                }

                var dp = Department.GetOne(departID);
                if (dp == null)
                    return Failure("该部门不存在");
                if (dp.Name == "根部门")
                    return Failure("根部门不能删除");
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
                //判断下级部门和下级人员
                if(Department.GetChildren(departID).Count>0)
                    return Failure("该部门有下级部门，请先处理子部门");
                if(UserInfo.GetUserByDepartment(departID).Count>0)
                    return Failure("该部门有下级员工，请先处理这些员工");
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
                if (search == null)
                    return BadRequest();
                if (search.PageIndex == 0)
                    search.PageIndex = 1;
                if (search.PageSize == 0)
                    search.PageSize = 20;
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                {
                    return Logout();
                }
                List<Department> departList = Department.GetAllChildren(userCookie.DepartmentID);
                if (!string.IsNullOrEmpty(search.DepartmentName))
                    departList.FindAll(m => m.Name.Contains(search.DepartmentName)); 
                var result = Department.GetDepartment(search, departList.Select(m=>m.ID).ToList());
                if (result.Items.Count > 0)
                {
                    var list = Department.GetAll(result.Items.ToList().Select(m => m.ID).ToList());
                    result.Items.ForEach(item =>
                    {
                        item.ParentName = list.Find(m => m.ID == item.ParentID)?.Name;
                    });
                }
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