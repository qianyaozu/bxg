using CabinetAPI.Filter;
using CabinetData.Entities;
using CabinetData.Entities.Principal;
using CabinetData.Entities.QueryEntities;
using CabinetUtility;
using CabinetUtility.Encryption;
using Newtonsoft.Json;
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
        private Logger _logger = LogManager.GetLogger("CabinetController");
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

                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                {
                    return Logout();
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
                if (!string.IsNullOrEmpty(cabinet.FirstContactPassword))
                    cabinet.FirstContactPassword = AESAlgorithm.Encrypto(cabinet.FirstContactPassword);
                if (!string.IsNullOrEmpty(cabinet.SecondContactPassword))
                    cabinet.SecondContactPassword = AESAlgorithm.Encrypto(cabinet.SecondContactPassword);
                Cabinet.Add(cabinet);
                return Success(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Failure("新增失败");
            }
        }

        /// <summary>
        /// 验证用户实体类
        /// </summary>
        /// <param name="cabinet"></param>
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
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                {
                    return Logout();
                }
                if (cabinet.ID == 0)
                    return Failure("未指定保险柜");
                var cab = Cabinet.GetOne(cabinet.ID);
                if (cab == null)
                    return Failure("未找到指定保险柜");
                var old = Cabinet.GetByName(cabinet.Name);
                if (old != null && old.ID != cabinet.ID)
                    return Failure("该名称已经被使用");

                old = Cabinet.GetByMac(cabinet.AndroidMac);
                if (old != null && old.ID != cabinet.ID)
                    return Failure("该硬件编码已经被使用");


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
                _logger.Error(ex);
                return Failure("修改失败");
            }
        }

        /// <summary>
        /// 更新保险柜状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost, Route("api/cabinet/updatestatus")]
        public IHttpActionResult UpdateCabinetStatus(int id = 0, int status = -1)
        {
            if (id == 0 || status == -1)
                return BadRequest();
            if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                return Logout();
            UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
            if (userCookie == null)
                return Logout();
            try
            {
                var cab = Cabinet.GetOne(id);
                if (cab == null)
                    return Failure("该保险柜不存在");
                if (status == 1)
                {
                    status = 3;
                }
                cab.Status = status;
                Cabinet.Update(cab);
                return Success();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Failure("更新失败");
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
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                {
                    return Logout();
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
                _logger.Error(ex);
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

                if (search == null)
                    return BadRequest();
                if (search.PageIndex == 0)
                    search.PageIndex = 1;
                if (search.PageSize == 0)
                    search.PageSize = 20;
                _logger.Info(JsonConvert.SerializeObject(UserController.LoginDictionary));
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                {
                    return Logout();
                }

                //更新保险柜状态
                var cabinetList = Cabinet.GetAll().FindAll(m => (m.Status == 1 || m.Status == 4));
                foreach (var m in cabinetList)
                {
                    var log = CabinetLog.GetOpenLog(m.ID);
                    if (log == null || log.CreateTime.AddSeconds(60) < DateTime.Now)
                    {
                        m.Status = 3;
                        Cabinet.Update(m);
                        _logger.Info("自动重置关闭");
                    }
                }

                List<Department> departList = Department.GetAllChildren(userCookie.DepartmentID);
                if (!string.IsNullOrEmpty(search.DepartmentName))
                    departList = departList.FindAll(m => m.Name.Contains(search.DepartmentName));
                var result = Cabinet.GetCabinets(search, departList.Select(m => m.ID).ToList());
                if (result.Items.Count > 0)
                {
                    var depart = Department.GetAll(result.Items.Select(m => m.DepartmentID).ToList());
                    result.Items.ForEach(m =>
                    {
                        m.DepartmentName = depart.Find(n => n.ID == m.DepartmentID)?.Name;
                        m.FirstContactPassword = string.IsNullOrEmpty(m.FirstContactPassword) ? "" : AESAlgorithm.Decrypto(m.FirstContactPassword);
                        m.SecondContactPassword = string.IsNullOrEmpty(m.SecondContactPassword) ? "" : AESAlgorithm.Decrypto(m.SecondContactPassword);
                    });
                }
                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Failure("查询失败");
            }
        }

        /// <summary>
        /// 根据部门获取保险柜
        /// </summary>
        /// <param name="departID"></param>
        /// <returns></returns>
        [HttpPost, Route("api/cabinet/viewbydepart")]
        public IHttpActionResult QueryCabinetByDepartment(int departID)
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
                List<int> departList = Department.GetAllChildren(userCookie.DepartmentID).Select(m => m.ID).ToList();//我的权限所能看到的部门
                List<int> departList1 = Department.GetAllChildren(departID).Select(m => m.ID).ToList();//查询指定部门所能看到的部门，取交集
                var result = Cabinet.GetCabinetsByDepart(departList.Intersect(departList1).ToList());
                if (result.Count > 0)
                {
                    var depart = Department.GetAll(result.Select(m => m.DepartmentID).ToList());
                    result.ForEach(m =>
                    {
                        m.DepartmentName = depart.Find(n => n.ID == m.DepartmentID)?.Name;
                    });
                }
                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Failure("查询失败");
            }
        }

        /// <summary>
        /// 获取保险柜分布图
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("api/cabinet/map")]
        public IHttpActionResult CabinetMap()
        {
            if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                return Logout();
            UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
            if (userCookie == null)
            {
                return Logout();
            }
            try
            {
                List<Cabinet> cabinetList = Cabinet.GetAll();
                List<Department> departList = Department.GetAll();
                Department root = departList.Find(m => m.ID == userCookie.DepartmentID);
                return Success(FindCabinetTree(root, departList, cabinetList));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Failure(ex.Message);
            }
        }
        private CabinetTree FindCabinetTree(Department root, List<Department> departList, List<Cabinet> cabinets)
        {
            CabinetTree tree = new CabinetTree();
            tree.id = 100000000 + root.ID;
            tree.type = 0;
            tree.name = root.Name;
            tree.children = new List<CabinetTree>();
            departList.FindAll(m => m.ParentID == root.ID).ForEach(m =>
            {
                tree.children.Add(FindCabinetTree(m, departList, cabinets));
            });
            cabinets.FindAll(m => m.DepartmentID == root.ID).ForEach(m =>
            {
                tree.children.Add(new CabinetTree
                {
                    id = m.ID,
                    type = 1,
                    name = m.Name,
                    data = new
                    {
                        warning = (m.Status != 0)
                    }
                });
            });
            return tree;
        }
    }
}