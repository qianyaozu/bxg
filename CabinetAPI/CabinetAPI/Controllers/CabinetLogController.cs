using CabinetAPI.Filter;
using CabinetAPI.Models;
using CabinetData.Entities;
using CabinetData.Entities.Principal;
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
                List<Department> departList = Department.GetAllChildren(userCookie.DepartmentID);
                var result = CabinetLog.GetCabinets(search, departList.Select(m=>m.ID).ToList());
                if (result.Items.Count > 0)
                {
                    var cabinet = Cabinet.GetCabinetByIds(result.Items.Select(m => m.CabinetID).ToList());
                    var depart = Department.GetAll(result.Items.Select(m => m.DepartmentID ?? 0).ToList());
                    result.Items.ForEach(m =>
                    {
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
                var cache = CacheHelper.GetCache(GetCookie("token"));
                if (cache == null)
                    return Logout();
                UserInfo userCookie = cache as UserInfo;
                if (userCookie == null)
                    return Logout();
                DateTime lastTime = ConvertStringToDateTime(time);
                var list = new List<CabinetLog>();
                List<CommandModel> cmdList = new List<CommandModel>();
                var departList = Department.GetChildren(userCookie.DepartmentID).Select(m => m.ID).ToList();
                lock (AndroidController.logLock)
                    list = AndroidController.CabinetLogQueue.ToList().FindAll(m => m.CreateTime >= lastTime && departList.Contains(m.DepartmentID ?? 0));

                if (list.Count == 0)
                    return Success(cmdList);

                var allDepartList = Department.GetDepartmentByIds(list.Select(m => m.DepartmentID ?? 0).ToList());
                var allCanbinet = Cabinet.GetCabinetByIds(list.Select(m => m.CabinetID).ToList());
                list.ForEach(m =>
                {
                    CommandModel cmd = new CommandModel();
                    cmd.CabinetID = m.CabinetID;
                    cmd.CabinetMac = allCanbinet.Find(n => n.ID == m.CabinetID)?.AndroidMac;
                    cmd.CabinetName = allCanbinet.Find(n => n.ID == m.CabinetID)?.Name;
                    cmd.DepartmentName = allDepartList.Find(n => n.ID == m.DepartmentID)?.Name;
                    cmd.OperateTime = m.OperateTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    cmd.OperatorName = m.OperatorName;
                    cmd.OperationType = m.OperationType;
                    cmd.EventContent = m.EventContent; 
                    cmd.WindowType = 0;
                    if (m.OperationType == 2 || m.OperationType == 4 || m.OperationType == 5 || m.OperationType == 6 || m.OperationType == 7 || m.OperationType == 8 || m.OperationType == 9 || m.OperationType == 10)
                        cmd.WindowType = 1;
                    if (m.OperationType == 15 || m.OperationType == 0)
                        cmd.WindowType = 2;
                    cmdList.Add(cmd);
                });
                return Success(cmdList);
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
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
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
            if (query == null)
                return BadRequest();
            try
            {
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
            if (query == null)
                return BadRequest();
            try
            {
                
                var result = CabinetLog.DepartMonthAlarmStatistics(query.DepartmentID, query.From, query.To );
                return Success(result);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("提交失败");
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
            if (query == null)
                return BadRequest();
            try
            {
                
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