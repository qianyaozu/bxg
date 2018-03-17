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
    public class CabinetLogController: BaseController
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
                return Success(result);
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
                var departList = Department.GetChildrenID(new List<int>() { userCookie.DepartmentID });
                lock (AndroidController.logLock)
                    list = AndroidController.CabinetLogQueue.ToList().FindAll(m => m.CreateTime >= lastTime && departList.Contains(m.DepartmentID ?? 0));
                return Success(list);
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
        /// 提交拒绝语音日志
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        [HttpPost, Route("api/cabinetlog/rejectaudio")]
        public IHttpActionResult RejectAudio(CabinetLog log)
        {
            if (log == null)
                return BadRequest();
            try
            {
                var cache = CacheHelper.GetCache(GetCookie("token"));
                if (cache == null)
                    return Logout();
                UserInfo userCookie = cache as UserInfo;
                if (userCookie == null)
                    return Logout();
                log.OperationType = (int)OperatorTypeEnum.拒绝语音;
                log.OperatorName = userCookie.Name;
                log.DepartmentID = userCookie.DepartmentID;
                log.EventContent = "拒绝语音";
                log.OperateTime = DateTime.Now;
                log.CreateTime = DateTime.Now;
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
        /// 提交接受语音日志
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        [HttpPost, Route("api/cabinetlog/acceptaudio")]
        public IHttpActionResult AcceptAudio(CabinetLog log)
        {
            if (log == null)
                return BadRequest();
            try
            {
                var cache = CacheHelper.GetCache(GetCookie("token"));
                if (cache == null)
                    return Logout();
                UserInfo userCookie = cache as UserInfo;
                if (userCookie == null)
                    return Logout();
                log.OperationType = (int)OperatorTypeEnum.接受语音;
                log.OperatorName = userCookie.Name;
                log.DepartmentID = userCookie.DepartmentID;
                log.EventContent = "接受语音";
                log.OperateTime = DateTime.Now;
                log.CreateTime = DateTime.Now;
                CabinetLog.Add(log);
                return Success();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("提交失败");
            }
        }
    }
}