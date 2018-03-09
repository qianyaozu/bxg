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
    [TokenFilter]
    public class SystemLogController: BaseController
    {


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