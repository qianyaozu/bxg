using CabinetAPI.Filter;
using CabinetData.Entities;
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
                    return Failure("登录失效");
                UserInfo userCookie = cache as UserInfo;
                if (userCookie == null)
                    return Failure("登录失效");
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
    }
}