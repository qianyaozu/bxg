using CabinetAPI.Controllers;
using CabinetData.Entities;
using CabinetData.Entities.Principal;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace CabinetAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public Logger logger = LogManager.GetLogger("WebApiApplication");
        private static string pingInterval = ConfigurationManager.AppSettings["PingInterval"]?.ToString();
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            InitDB();
            Task.Factory.StartNew(SyncCabinet);
        }
        private void SyncCabinet()
        {
            int interval = 30;
            if (!string.IsNullOrEmpty(pingInterval))
            {
                int.TryParse(pingInterval, out interval);
            }
            Thread.Sleep(interval*1000);
            while (true)
            {
                try
                {
                    //删除5分钟之前的命令
                    lock (AndroidController.logLock)
                        AndroidController.CabinetLogQueue.RemoveAll(m => m.CreateTime.AddMinutes(5) < DateTime.Now);
                    //List<string> keys = UserController.LoginDictionary.Keys.ToList();
                    //foreach (var key in keys)
                    //{
                    //    if (UserController.LoginDictionary.ContainsKey(key))
                    //    {
                    //        if(  UserController.LoginDictionary[key].LastLoginTime!=null&&(DateTime.Now- UserController.LoginDictionary[key].LastLoginTime.Value).TotalHours > 24)
                    //        {
                    //            logger.Error("自动退出 " + key + " " + UserController.LoginDictionary[key].LastLoginTime.Value.ToString("yyyy.MM.dd HH:mm:ss"));
                    //            UserController.LoginDictionary.Remove(key);
                    //        }
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                try
                {
                    //List<int> onLine = new List<int>();
                    //List<int> offLine = new List<int>();
                    List<CabinetData.Entities.CabinetLog> logs = new List<CabinetData.Entities.CabinetLog>();
                    var cabinetList = CabinetData.Entities.Cabinet.GetAll();
                    foreach (var cabinet in cabinetList)
                    {
                        bool isOnline = false;
                        if (cabinet.IsOnline ?? false)//在线
                        {
                            //如果保险柜在线，检测是否下线
                            if (!AndroidController.HeartDictionary.ContainsKey(cabinet.ID) || (AndroidController.HeartDictionary.ContainsKey(cabinet.ID) && AndroidController.HeartDictionary[cabinet.ID].AddSeconds(60) < DateTime.Now))
                            {
                                string host = cabinet.IP;
                                Ping p1 = new Ping();
                                PingReply reply = p1.Send(host); //发送主机名或Ip地址
                                if (reply.Status != IPStatus.Success)
                                {
                                    logger.Warn("03保险柜在线,心跳不在且ping不通下线" + cabinet.ID + ":" + cabinet.IP);
                                    //offLine.Add(cabinet.ID);
                                    var log = new CabinetData.Entities.CabinetLog
                                    {
                                        CabinetID = cabinet.ID,
                                        DepartmentID = cabinet.DepartmentID,
                                        OperatorName = "",
                                        OperateTime = DateTime.Now,
                                        OperationType = (int)OperatorTypeEnum.下线,
                                        CreateTime = DateTime.Now,
                                        CabinetIP = "",
                                        EventContent = "超时下线"
                                    };
                                    logs.Add(log);
                                    CabinetData.Entities.Cabinet.UpdateOffLine(new List<int> { cabinet.ID });
                                    lock (AndroidController.logLock)
                                    {
                                        AndroidController.msgID++;
                                        log.ID = AndroidController.msgID;
                                        AndroidController.CabinetLogQueue.Add(log);
                                    }
                                    DateTime dt;
                                    AndroidController.HeartDictionary.TryRemove(cabinet.ID, out dt);
                                }
                            }
                        }
                        else
                        {
                            if (AndroidController.HeartDictionary.ContainsKey(cabinet.ID) && AndroidController.HeartDictionary[cabinet.ID].AddSeconds(60) > DateTime.Now)//如果存在
                            {
                                logger.Warn("01保险柜不在线,心跳上线" + cabinet.ID + ":" + cabinet.IP);
                                isOnline = true;
                            }
                            else
                            {
                                string host = cabinet.IP;
                                Ping p1 = new Ping();
                                PingReply reply = p1.Send(host); //发送主机名或Ip地址
                                if (reply.Status == IPStatus.Success)
                                {
                                    logger.Warn("02保险柜不在线,ping上线" + cabinet.ID + ":" + cabinet.IP);
                                    isOnline = true;
                                }
                            }
                            if (isOnline)
                            {
                                //onLine.Add(cabinet.ID);
                                var log = new CabinetData.Entities.CabinetLog
                                {
                                    CabinetID = cabinet.ID,
                                    DepartmentID = cabinet.DepartmentID,
                                    OperatorName = "",
                                    OperateTime = DateTime.Now,
                                    OperationType = (int)OperatorTypeEnum.上线,
                                    CreateTime = DateTime.Now,
                                    CabinetIP = "",
                                    EventContent = ""
                                };
                                logs.Add(log);
                                CabinetData.Entities.Cabinet.UpdateOnLine(new List<int> { cabinet.ID });
                                lock (AndroidController.logLock)
                                {
                                    AndroidController.msgID++;
                                    log.ID = AndroidController.msgID;
                                    AndroidController.CabinetLogQueue.Add(log);
                                }
                            }
                            
                        } 
                    }
                    if (logs.Count > 0)
                        CabinetLog.Add(logs);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    Thread.Sleep(interval*1000);
                }
            }
        }

        private void InitDB()
        {
            #region 初始化部门
            Department depart = Department.GetOne("根组织");
            if (depart == null)
            {
                Department.Add(new Department
                {
                    Name = "根组织",
                    ParentID = null,
                    Remark = "不可删除",
                    SortID = 0
                });
                depart = Department.GetOne("根组织");
            }
            #endregion
            #region 初始化管理员
            UserInfo user = UserInfo.GetOne("admin");
            if (user == null)
            {
                UserInfo.Add(new UserInfo
                {
                    Name = "admin",
                    RoleID = 1,
                    DepartmentID = depart.ID,
                    Password = CabinetUtility.Encryption.AESAlgorithm.Encrypto("admin"),
                    RealName = "管理员",
                    CreateTime = DateTime.Now,
                    Status = 1,
                    LastPasswordTime = DateTime.Now
                });
            }
            #endregion

            #region 初始化权限
            if (Role_Module.Get(1).Count == 0)
            {
                List<Role_Module> roleList = new List<Role_Module>();
                for (int i = 1; i < 5; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        Role_Module role = new CabinetData.Entities.Role_Module();
                        role.RoleID = i;
                        role.ModuleID = j;
                        role.ModuleName = Enum.GetName(typeof(ModuleEnum), j);
                        role.EnableAdd = true;
                        role.EnableDelete = true;
                        role.EnableEdit = true;
                        role.EnableView = true;
                        roleList.Add(role);
                    }
                }
                Role_Module.Insert(roleList);
            }

            #endregion

            #region 新增字段
            UserInfo.AddColumn();
            #endregion

        }
    }
}
