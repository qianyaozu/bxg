using CabinetAPI.Filter;
using CabinetData.Entities;
using CabinetData.Entities.Principal;
using CabinetData.Entities.QueryEntities;
using CabinetUtility;
using CabinetUtility.Encryption;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CabinetAPI.Controllers
{
    /// <summary>
    /// 用户信息接口
    /// </summary>
    [TokenFilter]
    public class UserController : BaseController
    {
         
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns>返回权限集合</returns>
        [HttpPost, Route("api/user/login")]
        public IHttpActionResult Login(LoginModel model)
        {
            if (model == null)
                return Failure("用户名不存在");
            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
                return Failure("用户名或密码不得为空");
            UserInfo user = UserInfo.GetOne(model.UserName);
            if (user == null)
                return Failure("用户名不存在");
            if (user.Password != AESAlgorithm.Encrypto(model.Password))
                return Failure("密码错误");
            var token = Guid.NewGuid().ToString();
           

            
            SystemLog.Add(new SystemLog
            {
                Action = "Login",
                LogContent = user.Name + "-登录成功",
                CreateTime = DateTime.Now,
                UserID = user.ID,
                RoleID = user.RoleID,
                DepartmentID = user.DepartmentID,
                ClientIP = GetIP(),
                UserName = user.Name,
                RealName = user.RealName
            });
            Department depart = Department.GetOne(user.DepartmentID);
            var data = new
            {
                UserID = user.ID,
                RoleName = user.RoleID == 1 ? "admin" : "user",//1是超管，2是用户
                RealName = user.RealName,
                DepartmentName= depart?.Name
                //RoleModel = Role_Module.Get(user.RoleID),
            };
             
            WriteCookie("token", token);
            CacheHelper.SetCache(token, user, new TimeSpan(48, 0, 0)); 
            return Success(data);//返回用户权限
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("api/user/logout")]
        public IHttpActionResult Logout(LoginModel model)
        {
            UserInfo user= CacheHelper.GetCache(GetCookie("token") ) as UserInfo;
            if (user==null)
            {
                return Logout();
            }
            SystemLog.Add(new SystemLog
            {
                Action = "Logout",
                LogContent = user.Name + "-退出登录",
                CreateTime = DateTime.Now,
                UserID = user.ID,
                RoleID = user.RoleID,
                DepartmentID = user.DepartmentID,
                ClientIP = GetIP(),
                UserName = user.Name,
                RealName = user.RealName
            });
            CacheHelper.SetCache(GetCookie("token"), null, new TimeSpan(0, 0, 1));
           
            WriteCookie("token", "", -1);
            return Success(true);
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost, Route("api/user/add")]
        public IHttpActionResult AddUser(UserInfo user)
        {
            try
            {
                
                string valiate = ValiateUserModel(user);
                if (!string.IsNullOrEmpty(valiate))
                {
                    return Failure(valiate);
                }
                if (UserInfo.GetOne(user.Name) != null)
                {
                    return Failure("该用户名已经存在");
                }


                UserInfo userCookie = CacheHelper.GetCache(GetCookie("token")) as UserInfo;
                if (userCookie == null)
                {
                    return Logout();
                }
                SystemLog.Add(new SystemLog
                {
                    Action = "AddUser",
                    LogContent = userCookie.Name + "-新增用户-" + user.Name,
                    CreateTime = DateTime.Now,
                    UserID = userCookie.ID,
                    RoleID = userCookie.RoleID,
                    DepartmentID = userCookie.DepartmentID,
                    ClientIP = GetIP(),
                    UserName = userCookie.Name,
                    RealName = userCookie.RealName
                });

                user.CreateTime = DateTime.Now;
                user.RoleID = (int)RoleEnum.管理员;
                UserInfo.Add(user);
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
        /// <param name="user"></param>
        /// <returns></returns>
        private string ValiateUserModel(UserInfo user)
        {
            if (user == null)
                return "参数错误";
            if (user.DepartmentID == 0)
                return "请指定部门";
            if (string.IsNullOrEmpty(user.Name))
                return "请设置用户名称";
            if (string.IsNullOrEmpty(user.Password))
                return "请设置用户密码";
            if (string.IsNullOrEmpty(user.RealName))
                return "请设置用户真实姓名";
            return null;
        }

        /// <summary>
        /// 编辑用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost, Route("api/user/edit")]
        public IHttpActionResult EditUser(UserInfo user)
        {
            try
            {
               
                string valiate = ValiateUserModel(user);
                if (!string.IsNullOrEmpty(valiate))
                {
                    return Failure(valiate);
                }
                if (user.ID == 0)
                    return Failure("未指定用户");
                var us = UserInfo.GetOne(user.ID);
                if (us == null)
                    return Failure("未找到指定用户");

                var old = UserInfo.GetOne(user.Name);
                if (old != null&&old.ID!=user.ID)
                {
                    return Failure("该用户名已经被使用");
                }

                UserInfo userCookie = CacheHelper.GetCache(GetCookie("token")) as UserInfo;
                if (userCookie == null)
                {
                    return Logout();
                }
                SystemLog.Add(new SystemLog
                {
                    Action = "EditUser",
                    LogContent = userCookie.Name + "-编辑用户-" + user.Name,
                    CreateTime = DateTime.Now,
                    UserID = userCookie.ID,
                    RoleID = userCookie.RoleID,
                    DepartmentID = userCookie.DepartmentID,
                    ClientIP = GetIP(),
                    UserName = userCookie.Name,
                    RealName = userCookie.RealName
                });

                us.Name = user.Name;
                us.Password = AESAlgorithm.Encrypto(user.Password);
                us.DepartmentID = user.DepartmentID;
                us.RealName = user.RealName;
                us.Status = user.Status;
                us.Phone = user.Phone;
                us.Email = user.Email;
                UserInfo.Update(us);
                return Success(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("修改失败");
            }
        }
        [HttpPost, Route("api/user/updatestatus")]
        public IHttpActionResult UpdateUserStatus(int ID)
        {

            try
            {
                var user = UserInfo.GetOne(ID);
                if (user == null)
                    return Failure("未找到该用户");
                if (user.Status == 0)
                    user.Status = 1;
                else
                    user.Status = 0;
                UserInfo.Update(user);
                return Success();

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("执行异常");
            }
        }
        [HttpPost, Route("api/user/resetpassword")]
        public IHttpActionResult ResetPassword(int id)
        {
            
            try
            {
                var user = UserInfo.GetOne(id);
                if (user == null)
                    return Failure("未找到该用户");
                user.Password = AESAlgorithm.Encrypto("123456");
                UserInfo.Update(user);
                return Success();

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("执行异常");
            }
        }


        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        [HttpPost, Route("api/user/delete")]
        public IHttpActionResult DeleteUser(int UserID)
        {
            if (UserID == 0)
            {
                return Failure("未指定用户");
            }

            try
            {
                UserInfo userCookie = CacheHelper.GetCache(GetCookie("token")) as UserInfo;
                if (userCookie == null)
                {
                    return Logout();
                }
                SystemLog.Add(new SystemLog
                {
                    Action = "DeleteUser",
                    LogContent = userCookie.Name + "-删除用户-" + UserID,
                    CreateTime = DateTime.Now,
                    UserID = userCookie.ID,
                    RoleID = userCookie.RoleID,
                    DepartmentID = userCookie.DepartmentID,
                    ClientIP = GetIP(),
                    UserName = userCookie.Name,
                    RealName = userCookie.RealName
                });
                UserInfo.Delete(UserID);
                return Success(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("删除失败");
            }
        }

        /// <summary>
        /// 分页查询用户
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpPost, Route("api/user/view")]
        public IHttpActionResult QueryUser(UserSearchModel search)
        {
            try
            {
                if (search == null)
                    return BadRequest();
                if (search.PageIndex == 0)
                    search.PageIndex = 1;
                if (search.PageSize == 0)
                    search.PageSize = 20;
                var result = UserInfo.GetUsers(search);
                if (result.Items.Count > 0)
                {
                    var depart = Department.GetAll(result.Items.Select(m => m.DepartmentID).ToList());
                    result.Items.ForEach(m =>
                    {
                        m.DepartmentName = depart.Find(n => n.ID == m.DepartmentID)?.Name;
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