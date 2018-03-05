using CabinetAPI.Controllers;
using CabinetData.Entities;
using CabinetData.Entities.Principal;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace CabinetAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public Logger logger = LogManager.GetCurrentClassLogger();
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Task.Factory.StartNew(SyncCabinet);
        }
        private void SyncCabinet()
        {
            Thread.Sleep(30000);
            while (true)
            {
                try
                {
                    List<int> onLine = new List<int>();
                    List<int> offLine = new List<int>();
                    List<CabinetData.Entities.CabinetLog> logs = new List<CabinetData.Entities.CabinetLog>();
                    var cabinetList = CabinetData.Entities.Cabinet.GetAll();
                    foreach (var cabinet in cabinetList)
                    {
                        if (cabinet.IsOnline ?? false)//不在线
                        {
                            if (AndroidController.heartDictionary.ContainsKey(cabinet.ID) && AndroidController.heartDictionary[cabinet.ID].AddSeconds(30) > DateTime.Now)//如果存在
                            {
                                //上线
                                onLine.Add(cabinet.ID);
                                logs.Add(new CabinetData.Entities.CabinetLog
                                {
                                    CabinetID = cabinet.ID,
                                    OperatorName = "",
                                    OperateTime = DateTime.Now,
                                    OperationType = (int)OperatorTypeEnum.上线,
                                    CreateTime = DateTime.Now,
                                    CabinetIP = "",
                                    EventContent = ""
                                });
                            }
                        }
                        else if ((AndroidController.heartDictionary.ContainsKey(cabinet.ID) && AndroidController.heartDictionary[cabinet.ID].AddSeconds(30) < DateTime.Now) || !AndroidController.heartDictionary.ContainsKey(cabinet.ID))
                        {
                            //下线
                            offLine.Add(cabinet.ID);
                            logs.Add(new CabinetData.Entities.CabinetLog
                            {
                                CabinetID = cabinet.ID,
                                OperatorName = "",
                                OperateTime = DateTime.Now,
                                OperationType = (int)OperatorTypeEnum.下线,
                                CreateTime = DateTime.Now,
                                CabinetIP = "",
                                EventContent = "超时下线"
                            });
                        }
                    }
                    if (onLine.Count > 0)
                        CabinetData.Entities.Cabinet.UpdateOnLine(onLine);
                    if (offLine.Count > 0)
                        CabinetData.Entities.Cabinet.UpdateOffLine(offLine);
                    if (logs.Count > 0)
                        CabinetLog.Add(logs);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    Thread.Sleep(30000);
                }
            }
        }
    }
}
