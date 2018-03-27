using CabinetAPI.Filter;
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
                var result = CabinetLog.GetCabinets(search);
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
                List<CommandModel> cmdList = new List<Controllers.CommandModel>();
                var departList = Department.GetChildrenID(new List<int>() { userCookie.DepartmentID });
                lock (AndroidController.logLock)
                    list = AndroidController.CabinetLogQueue.ToList().FindAll(m => m.CreateTime >= lastTime && departList.Contains(m.DepartmentID ?? 0));

                if (list.Count == 0)
                    return Success(cmdList);

                var allDepartList = Department.GetDepartmentByIds(list.Select(m => m.DepartmentID ?? 0).ToList());
                var allCanbinet = Cabinet.GetCabinetByIds(list.Select(m => m.CabinetID).ToList());
                list.ForEach(m =>
                {
                    CommandModel cmd = new Controllers.CommandModel();
                    cmd.CabinetID = m.CabinetID;
                    cmd.CabinetMac = allCanbinet.Find(n => n.ID == m.CabinetID)?.AndroidMac;
                    cmd.CabinetName = allCanbinet.Find(n => n.ID == m.CabinetID)?.Name;
                    cmd.DepartmentName = allDepartList.Find(n => n.ID == m.DepartmentID)?.Name;
                    cmd.OperateTime = m.OperateTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    cmd.OperatorName = m.OperatorName;
                    cmd.OperationType = m.OperationType;
                    cmd.EventContent = m.EventContent;
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
                var result = CabinetLog.DepartAlarmStatistics(query.DepartmentID, query.From  , query.To );
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
                var result = CabinetLog.DepartMonthAlarmStatistics(query.DepartmentID, query.From, query.To);
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
                var result = CabinetLog.DepartAlarmTypeStatistics(query.DepartmentID, query.From, query.To);
                return Success(result);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("提交失败");
            }
        }
    }


    public class DepartAlarmStatisticsModel
    {
        public string From { get; set; }

        public string To { get; set; }
        public int DepartmentID { get; set; }
    }


    public class CommandDeal
    {
        public int CabinetID { get; set; }
        
        public int OperationType { get; set; } 
        /// <summary>
        /// 处理意见
        /// </summary>
        public string EventContent { get; set; }

        public string Remark { get; set; } 

    }


    /// <summary>
    /// 命令信息
    /// </summary>
    public class CommandModel
    {
        public int CabinetID { get; set; }
        /// <summary>
        /// mac地址,语音房间号 =mac
        /// </summary>
        public String CabinetMac { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; }
        /// <summary>
        /// 保险柜名称
        /// </summary>
        public string CabinetName { get; set; }
        /// <summary>
        /// 操作人姓名
        /// </summary>
        public string OperatorName { get; set; }
        /// <summary>
        /// 发生时间
        /// </summary>
        public string OperateTime { get; set; }
        /// <summary>
        /// 命令类型
        /// 正常开门 = 1,
        /// 密码错误 = 2,
        /// 正常关门 = 3,
        /// 非工作时间开门 = 4,
        /// 非工作时间关门 = 5,
        /// 外部电源断开 = 6,
        /// 备份电源电压低 = 7,
        /// 未按规定关门 = 8,
        /// 强烈震动 = 9,
        /// 网络断开 = 10
        /// 请求语音 = 15,
        /// 结束语音 = 16,
        /// </summary>
        public int OperationType { get; set; }
        /// <summary>
        /// 事件信息
        /// </summary>
        public string EventContent { get; set; }
    }
}