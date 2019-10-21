using CabinetAPI.Filter;
using CabinetAPI.Models;
using CabinetData.Entities;
using CabinetData.Entities.Principal;
using CabinetData.Entities.QueryEntities;
using CabinetUtility;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace CabinetAPI.Controllers
{
    /// <summary>
    /// 保险柜日志
    /// </summary>
    [TokenFilter]
    public class CabinetLogController : BaseController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 分页查询保险柜日志
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>

        [HttpPost, Route("api/cabinetlog/view")]
        public IHttpActionResult QueryCabinetLog(CabinetLogSearchModel search)
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
                List<Department> departList = new List<Department>();
                if ((search.DepartmentID ?? 0) != 0)
                    departList = Department.GetAllChildren(search.DepartmentID.Value);
                else
                    departList = Department.GetAllChildren(userCookie.DepartmentID);
                var result = CabinetLog.GetCabinets(search, departList.Select(m => m.ID).ToList());
                if (result.Items.Count > 0)
                {
                    var cabinet = Cabinet.GetCabinetByIds(result.Items.Select(m => m.CabinetID).ToList());
                    var depart = Department.GetAll(result.Items.Select(m => m.DepartmentID ?? 0).ToList());
                    result.Items.ForEach(m =>
                    {
                        m.OperationType =  ((m.OperationType == 1 || m.OperationType == 4) ? (int)OperatorTypeEnum.申请开门 : m.OperationType);
                        m.CabinetName = cabinet.Find(n => n.ID == m.CabinetID)?.Name;
                        m.DepartmentName = depart.Find(n => n.ID == m.DepartmentID)?.Name;
                        m.CabinetCode = cabinet.Find(n => n.ID == m.CabinetID)?.Code;
                        if (!string.IsNullOrEmpty(m.EventContent)&& m.EventContent.ToLower().Contains("photos"))
                        {
                            //有事件
                            CommandRequest req = JsonConvert.DeserializeObject<CommandRequest>(m.EventContent);
                            if (req != null && req.Photos != null && req.Photos.Count > 0)
                            {
                                for (int i = 0; i < req.Photos.Count; i++)
                                {
                                    req.Photos[i] = ImgToBase64String(req.Photos[i]);
                                }
                                m.EventContent = JsonConvert.SerializeObject(req);
                            }
                        }
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


        [HttpPost, Route("api/cabinetlog/delete")]
        public IHttpActionResult DeleteCabinetLog(List<int> ids)
        {
            try
            {
                if (ids?.Count == 0)
                    return BadRequest();
                CabinetLog.Delete(ids);
                return Success();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Failure("查询失败");
            }
        }

        /// <summary>
        /// 根据时间戳获取实时消息
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [HttpGet, Route("api/cabinetlog/command")]
        public IHttpActionResult GetCommand(string time)
        {
            if (string.IsNullOrEmpty(time))
            {
                return Failure("时间戳不能为空");
            }
            try
            {
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")] ;
                if (userCookie == null)
                    return Logout();
                List<CommandModel> cmdList = new List<CommandModel>();
                var list = new List<CabinetLog>();
                int lastID = 0;
                if (!string.IsNullOrEmpty(time))
                {
                    if (time == "0" && AndroidController.CabinetLogQueue.Count>0)
                    {
                        time = AndroidController.CabinetLogQueue.Select(m => m.ID).Max().ToString();
                    }

                    try
                    {
                        lastID = Convert.ToInt32(time);
                    }
                    catch
                    {
                        lastID = 0;
                    }
                }
                int index = 0;
                while (index++ < 20)
                {
                    if (AndroidController.CabinetLogQueue.Count == 0)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        lock (AndroidController.logLock)
                        {
                            list = AndroidController.CabinetLogQueue.ToList().FindAll(m => m.ID > lastID);
                        }
                        if (list.Count == 0)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }

                if (AndroidController.CabinetLogQueue.Count == 0)
                    return Success(new
                    {
                        list = cmdList,
                        time = time,
                        msg="queue is empty"
                    });
                
                //_logger.Warn("enter" + AndroidController.CabinetLogQueue.Count);
               
               

                //var departList = Department.GetChildren(userCookie.DepartmentID).Select(m => m.ID).ToList();
                //departList.Add(userCookie.DepartmentID);
                //lock (AndroidController.logLock)
                //{
                //    list = AndroidController.CabinetLogQueue.ToList().FindAll(m => departList.Contains(m.DepartmentID ?? 0));
                //    if (list.Count > 0)
                //    {
                //        foreach (var item in list)
                //        {
                //            AndroidController.CabinetLogQueue.Remove(item);
                //        }
                //    }
                //}

                lock (AndroidController.logLock)
                {
                    
                   list = AndroidController.CabinetLogQueue.ToList().FindAll(m => m.ID > lastID);
                    //_logger.Info(lastID + "->符合数量" + list.Count);
                    
                }
                //_logger.Trace( lastID + "->" + string.Join(",", AndroidController.CabinetLogQueue.Select(n => n.ID)) + "->符合数量" + list.Count);
                if (list.Count == 0)
                    return Success(new
                    {
                        list = cmdList,
                        time = time
                    });

                var allDepartList = Department.GetDepartmentByIds(list.Select(m => m.DepartmentID ?? 0).ToList());
                var allCanbinet = Cabinet.GetCabinetByIds(list.Select(m => m.CabinetID).ToList());
                list.ForEach(m =>
                {
                    var item = allCanbinet.Find(n => n.ID == m.CabinetID);
                    CommandModel cmd = new CommandModel();
                    cmd.CabinetID = m.CabinetID;
                    cmd.CabinetMac = item?.AndroidMac;
                    cmd.CabinetName = item?.Name;
                    cmd.DepartmentName = item?.Name;
                    cmd.OperateTime = m.OperateTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    cmd.OperatorName = m.OperatorName;
                    cmd.OperationType = m.OperationType;
                    cmd.EventContent = m.EventContent;
                    //if (!string.IsNullOrEmpty(m.EventContent) && m.EventContent.ToLower().Contains("photos"))
                    //{
                    //    //有事件
                    //    CommandRequest req = JsonConvert.DeserializeObject<CommandRequest>(m.EventContent);
                    //    if (req != null && req.Photos != null && req.Photos.Count > 0)
                    //    {
                    //        for (int i = 0; i < req.Photos.Count; i++)
                    //        {
                    //            req.Photos[i] = ImgToBase64String(req.Photos[i]);
                    //        }
                    //        cmd.EventContent = JsonConvert.SerializeObject(req);
                    //    }
                    //}
                    cmd.WindowType = 0;
                   
                    if (m.OperationType == 32 || m.OperationType == 15 || m.OperationType == 24 || m.OperationType == 40)
                        cmd.WindowType = 2;
                    else if(m.OperationType == 2 || m.OperationType == 4 || m.OperationType == 5 || m.OperationType == 6||
                    m.OperationType == 7 || m.OperationType == 8 || m.OperationType == 9 || m.OperationType == 10 ||
                    m.OperationType == 14 || m.OperationType == 30 || m.OperationType == 31 || m.OperationType == 35 || m.OperationType == 50)
                        cmd.WindowType = 1;
                    cmdList.Add(cmd);
                });
                var data = new
                {
                    list = cmdList,
                    time = list.Select(m => m.ID).Max().ToString()
                };
                return Success(data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Failure("通讯异常");
            }
        }

        private DateTime ConvertStringToDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            dtStart = dtStart.AddMilliseconds(Convert.ToInt64(timeStamp));
            return dtStart;
        }
        private string ConvertDateTimeToString(DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            long timeStamp = (long)(DateTime.Now - startTime).TotalMilliseconds; // 相差毫秒数
            return timeStamp.ToString();
        }



        /// <summary>
        /// 提交操作日志
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        [HttpPost, Route("api/cabinetlog/submit")]
        public IHttpActionResult SubmitLog(CommandDeal cmd)
        {
            if (cmd == null)
                return BadRequest();
            try
            {
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                    return Logout();
                _logger.Warn(JsonConvert.SerializeObject(cmd));
                if (cmd.OperationType == (int)OperatorTypeEnum.允许开门 
                    || cmd.OperationType == (int)OperatorTypeEnum.拒绝开门
                    || cmd.OperationType == (int)OperatorTypeEnum.接受语音 
                    || cmd.OperationType == (int)OperatorTypeEnum.拒绝语音
                    || cmd.OperationType == (int)OperatorTypeEnum.允许终端修改信息
                    || cmd.OperationType == (int)OperatorTypeEnum.拒绝终端修改信息
                     || cmd.OperationType == (int)OperatorTypeEnum.允许人员注册
                    || cmd.OperationType == (int)OperatorTypeEnum.拒绝人员注册)
                {
                    if (AndroidController.CommandDictionary.ContainsKey(cmd.CabinetID))
                        AndroidController.CommandDictionary[cmd.CabinetID] = cmd.OperationType;
                    else
                        AndroidController.CommandDictionary.TryAdd(cmd.CabinetID, cmd.OperationType);
                }
                CabinetLog log = new CabinetLog();
                log.CabinetID = cmd.CabinetID;
                log.OperationType = cmd.OperationType;
                log.OperatorName = userCookie.Name;
                log.DepartmentID = userCookie.DepartmentID;
                log.EventContent = cmd.EventContent;
                log.OperateTime = DateTime.Now;
                log.CreateTime = DateTime.Now;
                log.Remark = cmd.Remark;
                CabinetLog.Add(log);
                if (cmd.OperationType == (int)OperatorTypeEnum.解除报警)
                {
                    var cab = Cabinet.GetOne(cmd.CabinetID);
                    if (cab != null)
                    {
                        cab.Alarm = "";
                        cab.Status = 23;
                        Cabinet.Update(cab);
                    }
                }
                return Success();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Failure("提交失败");
            }
        }

        /// <summary>
        /// 获取单位报警统计
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        [HttpPost, Route("api/cabinetlog/departAlarmStatistics")]
        public IHttpActionResult DepartAlarmStatistics(DepartAlarmStatisticsModel query)
        {
            if (query == null || string.IsNullOrEmpty(query.From) || string.IsNullOrEmpty(query.To))
                return BadRequest();
            try
            {
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                    return Logout();
                if (query.DepartmentID == 0)
                    query.DepartmentID = userCookie.DepartmentID;
                query.From = Convert.ToDateTime(query.From).ToString("yyyy-MM-dd 00:00:00");
                query.To = Convert.ToDateTime(query.To).ToString("yyyy-MM-dd  23:59:59");
                var result = CabinetLog.DepartAlarmStatistics(query.DepartmentID, query.From, query.To);
                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Failure("提交失败");
            }
        }


        /// <summary>
        /// 获取每月报警统计
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        [HttpPost, Route("api/cabinetlog/departMonthAlarmStatistics")]
        public IHttpActionResult DepartMonthAlarmStatistics(DepartAlarmStatisticsModel query)
        {


            if (query == null || string.IsNullOrEmpty(query.From) || string.IsNullOrEmpty(query.To))
                return BadRequest();
            try
            {
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                    return Logout();
                if (query.DepartmentID == 0)
                    query.DepartmentID = userCookie.DepartmentID;
                MonthAlarmStatisticsModel model = new Models.MonthAlarmStatisticsModel();
                model.Year = DateTime.Parse(query.From).Year.ToString();
                model.Data = new List<int>();

                query.From = Convert.ToDateTime(query.From).ToString("yyyy-MM-dd 00:00:00");
                query.To = Convert.ToDateTime(query.To).ToString("yyyy-MM-dd  23:59:59");

                var result = CabinetLog.DepartMonthAlarmStatistics(query.DepartmentID, query.From, query.To);
                if (result.Count > 0)
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        var item = result.Find(m => m.Month == i);
                        model.Data.Add(item == null ? 0 : item.Count);
                    }
                }
                return Success(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Failure("查询失败");
            }
        }

        /// <summary>
        /// 报警类型统计
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost, Route("api/cabinetlog/departAlarmTypeStatistics")]
        public IHttpActionResult DepartAlarmTypeStatistics(DepartAlarmStatisticsModel query)
        {
            if (query == null||string.IsNullOrEmpty(query.From)||string.IsNullOrEmpty(query.To))
                return BadRequest();
            try
            {
                if (!UserController.LoginDictionary.ContainsKey(GetCookie("token")))
                    return Logout();
                UserInfo userCookie = UserController.LoginDictionary[GetCookie("token")];
                if (userCookie == null)
                    return Logout();
                if (query.DepartmentID == 0)
                    query.DepartmentID = userCookie.DepartmentID;

                query.From = Convert.ToDateTime(query.From).ToString("yyyy-MM-dd 00:00:00");
                query.To = Convert.ToDateTime(query.To).ToString("yyyy-MM-dd  23:59:59");
                var result = CabinetLog.DepartAlarmTypeStatistics(query.DepartmentID, query.From, query.To );
                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Failure("提交失败");
            }
        }




        private string ImgToBase64String(string Imagefilename)
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + "upload\\"+  Imagefilename; 
                FileStream fsForRead = new FileStream(path, FileMode.Open); 
                fsForRead.Seek(0, SeekOrigin.Begin);
                byte[] bs = new byte[fsForRead.Length];
                int log = Convert.ToInt32(fsForRead.Length); 
                fsForRead.Read(bs, 0, log);
                fsForRead.Close();
                return "data:image/png;base64,"+Convert.ToBase64String(bs); 
            }
            catch (Exception ex)
            {
                return null;
            }
        }

         
    }


   

   
     
}