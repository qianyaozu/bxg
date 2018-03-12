using CabinetAPI.Filter;
using CabinetData.Entities;
using CabinetData.Entities.QueryEntities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CabinetAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [TokenFilter]
    public class SystemLogController: BaseController
    {

        /// <summary>
        /// 分页查询操作日志
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpPost, Route("api/systemlog/view")]
        public IHttpActionResult QuerySystemLog(SystemLogSearchModel search)
        {
            try
            {
                var result = SystemLog.GetSystemLogs(search);
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