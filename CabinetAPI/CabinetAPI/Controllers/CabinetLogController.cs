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
using System.Linq;
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
                UserInfo userCookie = CacheHelper.GetCache(GetCookie("token")) as UserInfo;
                if (userCookie == null)
                {
                    return Logout();
                }
                List<Department> departList = new List<Department>();
                if((search.DepartmentID??0)!=0)
                   departList= Department.GetAllChildren(search.DepartmentID.Value);
                else
                    departList = Department.GetAllChildren(userCookie.DepartmentID);
                var result = CabinetLog.GetCabinets(search, departList.Select(m=>m.ID).ToList());
                if (result.Items.Count > 0)
                {
                    var cabinet = Cabinet.GetCabinetByIds(result.Items.Select(m => m.CabinetID).ToList());
                    var depart = Department.GetAll(result.Items.Select(m => m.DepartmentID ?? 0).ToList());
                    result.Items.ForEach(m =>
                    {
                        m.CabinetName= cabinet.Find(n => n.ID == m.CabinetID)?.Name;
                        m.DepartmentName = depart.Find(n => n.ID == m.DepartmentID)?.Name;
                        m.CabinetCode = cabinet.Find(n => n.ID == m.CabinetID)?.Code;
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
                logger.Error(ex);
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
                var cookie = GetCookie("token");
                var cache = CacheHelper.GetCache(cookie);
                if (cache == null)
                    return Logout();
                UserInfo userCookie = cache as UserInfo;
                if (userCookie == null)
                    return Logout();
                List<CommandModel> cmdList = new List<CommandModel>();
                if (AndroidController.CabinetLogQueue.Count == 0)
                    return Success(new
                    {
                        list = cmdList,
                        time = ConvertDateTimeToString(DateTime.Now)
                    });
                DateTime lastTime = ConvertStringToDateTime(time);
                var list = new List<CabinetLog>();

                var departList = Department.GetChildren(userCookie.DepartmentID).Select(m => m.ID).ToList();
                departList.Add(userCookie.DepartmentID);
                lock (AndroidController.logLock)
                {
                    list = AndroidController.CabinetLogQueue.ToList().FindAll(m => departList.Contains(m.DepartmentID ?? 0));
                    if (list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            AndroidController.CabinetLogQueue.Remove(item);
                        }
                    }
                }

                //lock (AndroidController.logLock)
                //    list = AndroidController.CabinetLogQueue.ToList().FindAll(m => m.CreateTime > lastTime && departList.Contains(m.DepartmentID ?? 0));
                logger.Info(time + "   " + lastTime.ToString("yyyy-MM-dd HH:mm:ss") + " "   + AndroidController.CabinetLogQueue.Count + "/" + list.Count);
                if (list.Count == 0)
                    return Success(new
                    {
                        list = cmdList,
                        time = ConvertDateTimeToString(DateTime.Now)
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
                    cmd.WindowType = 0;
                    bool needConfirm = item.NeedConfirm ?? false;//是否需要开门审核
                    if (m.OperationType == 2 || (m.OperationType == 4 && !needConfirm) || m.OperationType == 5 || m.OperationType == 6 || m.OperationType == 7 || m.OperationType == 8 || m.OperationType == 9 || m.OperationType == 10|| m.OperationType == 14)
                        cmd.WindowType = 1;
                    if (m.OperationType == 15 || ((m.OperationType == 1 || m.OperationType == 4) && needConfirm))
                        cmd.WindowType = 2;
                    cmdList.Add(cmd);
                });
                var data = new
                {
                    list = cmdList,
                    time = ConvertDateTimeToString(list.Select(m => m.CreateTime).Max())
                };
                return Success(data);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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
                var cache = CacheHelper.GetCache(GetCookie("token"));
                if (cache == null)
                    return Logout();
                UserInfo userCookie = cache as UserInfo;
                if (userCookie == null)
                    return Logout();
                logger.Warn(JsonConvert.SerializeObject(cmd));
                if (cmd.OperationType == (int)OperatorTypeEnum.允许开门 || cmd.OperationType == (int)OperatorTypeEnum.拒绝开门|| cmd.OperationType == (int)OperatorTypeEnum.接受语音 || cmd.OperationType == (int)OperatorTypeEnum.拒绝语音)
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
                return Success();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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
                var cache = CacheHelper.GetCache(GetCookie("token"));
                if (cache == null)
                    return Logout();
                UserInfo userCookie = cache as UserInfo;
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
                logger.Error(ex);
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
                var cache = CacheHelper.GetCache(GetCookie("token"));
                if (cache == null)
                    return Logout();
                UserInfo userCookie = cache as UserInfo;
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
                logger.Error(ex);
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
                var cache = CacheHelper.GetCache(GetCookie("token"));
                if (cache == null)
                    return Logout();
                UserInfo userCookie = cache as UserInfo;
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
                logger.Error(ex);
                return Failure("提交失败");
            }
        }
    }


   

   
     
}