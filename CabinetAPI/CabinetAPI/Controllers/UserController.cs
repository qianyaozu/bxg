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
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;

namespace CabinetAPI.Controllers
{
    //    监管平台新增功能需求
    //1、增加角色，三元，包括系统管理员，安全管理员，安全审计员，将页面的功能划分给这三个角色，要求有相互监督，相互监管功能  x
    //2、所有的数据传输增加一个通讯加密
    //3、日志清理保存需要有一个安全保障
    //4、网页登录的用户名密码必须是字母+数字 （大于8位）   x
    //5、密码5次输入错误，不再给予尝试，并冻结             x
    //6、页面时效性，页面停留多久就需要重新登录            x 2小时缓存
    //7、用户密码时效性7天，加入在7天之后必须提示客户修改密码，可以用老密码登录进去修改成新密码  x
    //8、页面刷新报警要和保密柜状态一直，状态迁移到离线，声音必须马上报警。

    /// <summary>
    /// 用户信息接口
    /// </summary>
    [TokenFilter]
    public class UserController : BaseController
    {

        private Logger _logger = LogManager.GetLogger("UserController");
        /// <summary>
        /// 连续密码错误
        /// </summary>
        private static List<LoginModel> ContinueErrorPassword = new List<LoginModel>();

        public static Dictionary<string, UserInfo> LoginDictionary = new Dictionary<string, UserInfo>();
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
           
            //var serial = ConfigurationManager.AppSettings["SerialNumber"];
            //if (string.IsNullOrEmpty(serial))
            //{
            //    return Failure("请联系销售获取产品序列号");
            //}
            //DateTime dt = DateTime.Now;
            //if (!DateTime.TryParse(AESAlgorithm.Decrypto(serial),out dt)||dt < DateTime.Now)
            //{
            //    return Failure("序列号已经过期，请联系销售获取最新序列号");
            //}
            try
            {
                lock (ContinueErrorPassword)
                {
                    //校验5次密码错误
                    ContinueErrorPassword.RemoveAll(m => m.CreateTime.Day != DateTime.Now.Day);
                    if (ContinueErrorPassword.Count(m => m.UserName == model.UserName) > 5)
                    {
                        UserInfo u = UserInfo.GetOne(model.UserName);
                        if (u != null)
                        {
                            u.Status = 0;
                            UserInfo.Update(u);
                        }
                        return Failure("连续输错5次密码并冻结");
                    }
                }
                UserInfo user = UserInfo.GetOne(model.UserName);
                if (user == null)
                    return Failure("用户名不存在");
                if (user.Status == 0)
                    return Failure("此用户已禁用，请联系管理员");
                
                if (user.Password != AESAlgorithm.Encrypto(model.Password))
                {
                    model.CreateTime = DateTime.Now;
                    lock (ContinueErrorPassword)
                    {
                        ContinueErrorPassword.Add(model);
                    }
                    return Failure("密码错误");
                }
                var token = user.ID.ToString();


               
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
                if (user.LastPasswordTime == null)
                    user.LastPasswordTime = DateTime.Now;
                var data = new
                {
                    UserID = user.ID,
                    RoleName = user.RoleID == 1 ? "admin" : "user",//1是超管，2是用户
                    RealName = user.RealName,
                    DepartmentName = depart?.Name,
                    NeedChangePassword = (user.LastPasswordTime.Value.AddDays(7) < DateTime.Now ? true : false),//是否需要提示修改密码
                    RoleModel = Role_Module.Get(user.RoleID),//返回所有模块
                };

                WriteCookie("token", token);
                user.LastLoginTime = DateTime.Now;
                if (!LoginDictionary.ContainsKey(token))
                    LoginDictionary.Add(token, user);

                _logger.Info(  string.Join(",", LoginDictionary.Keys.ToList()));
                return Success(data);//返回用户权限
            }catch(Exception e)
            {
                _logger.Error(e);
                return Failure(e.Message);
            }
            
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("api/user/logout")]
        public IHttpActionResult Logout(LoginModel model)
        {
            if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
            {
                WriteCookie("token", "", -1);
                return Logout();
            }
            UserInfo user = UserController.LoginDictionary[GetCookie("token")];
            if (user == null)
            {
                WriteCookie("token", "", -1);
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
            LoginDictionary.Remove(GetCookie("token"));
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
                if (string.IsNullOrEmpty(user.Password))
                {
                    return Failure("密码不为空");
                }
                if (user.Password?.Length <= 8)
                {
                    return Failure("密码必须是大于8位");
                }
                if (!Regex.IsMatch(user.Password[0].ToString(), @"^[A-Za-z]"))
                {
                    return Failure("密码必须字母开头");
                }

                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
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
                user.RoleID = user.RoleID;
                user.Password = AESAlgorithm.Encrypto(user.Password);
                user.LastPasswordTime = DateTime.Now;
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
                if (string.IsNullOrEmpty(user.Password))
                {
                    return Failure("密码不为空");
                }
                if (user.Password?.Length <= 8)
                {
                    return Failure("密码必须是大于8位");
                }
                if (!Regex.IsMatch(user.Password[0].ToString(), @"^[A-Za-z]"))
                {
                    return Failure("密码必须字母开头");
                }

                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
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
                if(us.Password!= AESAlgorithm.Encrypto(user.Password))
                {
                    us.LastPasswordTime = DateTime.Now;
                }
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

        /// <summary>
        /// 修改自己密码
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost, Route("api/user/changepassword")]
        public IHttpActionResult ChangePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return Failure("密码不为空");
            }
            if (password?.Length <= 8)
            {
                return Failure("密码必须是大于8位");
            }
            if (!Regex.IsMatch(password[0].ToString(), @"^[A-Za-z]"))
            {
                return Failure("密码必须字母开头");
            }
            try
            {
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo user = UserController.LoginDictionary[GetCookie("token")];
                if (user == null)
                {
                    return Logout();
                }
                var us = UserInfo.GetOne(user.ID);
                us.Password = AESAlgorithm.Encrypto(password);
                UserInfo.Update(us);
                SystemLog.Add(new SystemLog
                {
                    Action = "Logout",
                    LogContent = user.Name + "-更新密码",
                    CreateTime = DateTime.Now,
                    UserID = user.ID,
                    RoleID = user.RoleID,
                    DepartmentID = user.DepartmentID,
                    ClientIP = GetIP(),
                    UserName = user.Name,
                    RealName = user.RealName
                });
                return Success();
            }catch(Exception ex)
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
                SystemLog.Add(new SystemLog
                {
                    Action = "Logout",
                    LogContent = user.Name + "-重置密码",
                    CreateTime = DateTime.Now,
                    UserID = user.ID,
                    RoleID = user.RoleID,
                    DepartmentID = user.DepartmentID,
                    ClientIP = GetIP(),
                    UserName = user.Name,
                    RealName = user.RealName
                });
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
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
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
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                {
                    return Logout();
                }
                List<Department> departList = Department.GetAllChildren(userCookie.DepartmentID);
                var result = UserInfo.GetUsers(search, departList.Select(m=>m.ID).ToList());
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