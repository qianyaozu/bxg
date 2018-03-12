using CabinetAPI.Filter;
using CabinetData.Entities;
using CabinetData.Entities.QueryEntities;
using CabinetUtility;
using CabinetUtility.Encryption;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CabinetAPI.Controllers
{
    /// <summary>
    /// 保险柜信息
    /// </summary>
    [TokenFilter]
    public class CabinetController : BaseController
    {
        /// <summary>
        /// 新增保险柜接口
        /// </summary>
        /// <param name="cabinet"></param>
        /// <returns></returns>
        [HttpPost, Route("api/cabinet/add")]
        public IHttpActionResult AddCabinet(Cabinet cabinet)
        {
            try
            {
                string valiate = ValiateCabinetModel(cabinet);
                if (!string.IsNullOrEmpty(valiate))
                {
                    return Failure(valiate);
                }
                if (Cabinet.GetByName(cabinet.Name) != null)
                    return Failure("该名称已经被使用");
                if (Cabinet.GetByMac(cabinet.AndroidMac) != null)
                    return Failure("该硬件编码已经被使用");


                UserInfo userCookie = CacheHelper.GetCache(GetCookie("token")) as UserInfo;
                if (userCookie == null)
                {
                    return Failure("登录失效");
                }
                SystemLog.Add(new SystemLog
                {
                    Action = "AddCabinet",
                    LogContent = userCookie.Name + "-新增保险柜-" + cabinet.Name,
                    CreateTime = DateTime.Now,
                    UserID = userCookie.ID,
                    RoleID = userCookie.RoleID,
                    DepartmentID = userCookie.DepartmentID,
                    ClientIP = GetIP(),
                    UserName = userCookie.Name,
                    RealName = userCookie.RealName
                });
                cabinet.CreateTime = DateTime.Now;
                cabinet.IsOnline = false;
                Cabinet.Add(cabinet);
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
        private string ValiateCabinetModel(Cabinet cabinet)
        {
            if (cabinet == null)
                return "参数错误";
            if (cabinet.DepartmentID == 0)
                return "请指定部门";
            if (string.IsNullOrEmpty(cabinet.Address))
                return "请设置所在地址";
            if (string.IsNullOrEmpty(cabinet.Name))
                return "请设置保险柜名称";
            if (string.IsNullOrEmpty(cabinet.Code))
                return "请设置保险柜编号";
            if (string.IsNullOrEmpty(cabinet.AndroidMac))
                return "请设置Android硬件地址";
            if (string.IsNullOrEmpty(cabinet.IP))
                return "请设置分配IP";
            return null;
        }

        /// <summary>
        /// 修改保险柜接口
        /// </summary>
        /// <param name="cabinet"></param>
        /// <returns></returns>
        [HttpPost, Route("api/cabinet/edit")]
        public IHttpActionResult EditCabinet(Cabinet cabinet)
        {
            try
            {
                string valiate = ValiateCabinetModel(cabinet);
                if (!string.IsNullOrEmpty(valiate))
                {
                    return Failure(valiate);
                }
                if (cabinet.ID == 0)
                    return Failure("未指定保险柜");
                var cab = Cabinet.GetOne(cabinet.ID);
                if (cab == null)
                    return Failure("未找到指定保险柜");
                var old = Cabinet.GetByName(cabinet.Name);
                if (old != null&& old.ID!=cabinet.ID) 
                    return Failure("该名称已经被使用");

                old = Cabinet.GetByMac(cabinet.AndroidMac);
                if (old != null && old.ID != cabinet.ID)
                    return Failure("该硬件编码已经被使用");

                UserInfo userCookie = CacheHelper.GetCache(GetCookie("token")) as UserInfo;
                if (userCookie == null)
                {
                    return Failure("登录失效");
                }
                SystemLog.Add(new SystemLog
                {
                    Action = "EditCabinet",
                    LogContent = userCookie.Name + "-编辑保险柜-" + cabinet.ID,
                    CreateTime = DateTime.Now,
                    UserID = userCookie.ID,
                    RoleID = userCookie.RoleID,
                    DepartmentID = userCookie.DepartmentID,
                    ClientIP = GetIP(),
                    UserName = userCookie.Name,
                    RealName = userCookie.RealName
                });
                cab.Address = cabinet.Address;
                cab.AndroidMac = cabinet.AndroidMac;
                cab.Code = cabinet.Code;
                cab.DepartmentID = cabinet.DepartmentID;
                cab.FirstContact = cabinet.FirstContact;
                cab.FirstContactPassword = AESAlgorithm.Encrypto(cabinet.FirstContactPassword);
                cab.FirstContactPhone = cabinet.FirstContactPhone;

                cab.IP = cabinet.IP;
                cab.Name = cabinet.Name;
                cab.NeedConfirm = cabinet.NeedConfirm;
                cab.Remark = cabinet.Remark;
                cab.SecondContact = cabinet.SecondContact;

                cab.SecondContactPassword = AESAlgorithm.Encrypto(cabinet.SecondContactPassword);
                cab.SecondContactPhone = cabinet.SecondContactPhone;

                Cabinet.Update(cab);
                return Success(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("修改失败");
            }
        }

        /// <summary>
        /// 删除保险柜接口
        /// </summary>
        /// <param name="cabinetID"></param>
        /// <returns></returns>
        [HttpPost, Route("api/cabinet/delete")]
        public IHttpActionResult DeleteCabinet(int cabinetID)
        {
            if (cabinetID == 0)
            {
                return Failure("未指定保险柜");
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
                    Action = "DeleteCabinet",
                    LogContent = userCookie.Name + "-删除保险柜-" + cabinetID,
                    CreateTime = DateTime.Now,
                    UserID = userCookie.ID,
                    RoleID = userCookie.RoleID,
                    DepartmentID = userCookie.DepartmentID,
                    ClientIP = GetIP(),
                    UserName = userCookie.Name,
                    RealName = userCookie.RealName
                });
                if (Cabinet.Delete(cabinetID))
                    return Success(true);
                return Failure("删除失败");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("删除失败");
            }
        }

        /// <summary>
        /// 分页查询保险柜接口
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpPost, Route("api/cabinet/view")]
        public IHttpActionResult QueryCabinet(CabinetSearchModel search)
        {
            try
            {
                var result = Cabinet.GetCabinets(search);
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