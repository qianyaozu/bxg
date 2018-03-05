using CabinetData.Entities;
using CabinetData.Entities.Principal;
using CabinetData.Entities.QueryEntities;
using CabinetUtility.Encryption;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CabinetAPI.Controllers
{
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
            WriteSession("UserID", user.ID.ToString());
            WriteSession("RoleID", user.RoleID.ToString());
            WriteSession("DepartmentID", user.DepartmentID.ToString());
            return Success(Role_Module.Get(user.RoleID));//返回用户权限
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("api/user/logout")]
        public IHttpActionResult Logout(LoginModel model)
        {
            RemoveSession("UserID");
            RemoveSession("RoleID");
            RemoveSession("DepartmentID");
            return Success(true);
        }

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

        [HttpPost, Route("api/user/delete")]
        public IHttpActionResult DeleteUser(int userID)
        {
            if (userID == 0)
            {
                return Failure("未指定用户");
            }
            try
            {
                UserInfo.Delete(userID);
                return Success(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("删除失败");
            }
        }

        [HttpPost, Route("api/user/view")]
        public IHttpActionResult QueryUser(UserSearchModel search)
        {
            try
            {
                var result = UserInfo.GetUsers(search);
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