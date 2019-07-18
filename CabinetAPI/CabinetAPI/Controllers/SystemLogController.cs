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
                List<Department> departList = Department.GetAllChildren(userCookie.DepartmentID);
                var result = SystemLog.GetSystemLogs(search, departList.Select(m=>m.ID).ToList());
                if (result.Items.Count > 0)
                {
                    var list = Department.GetAll(result.Items.ToList().Select(m => m.ID).ToList());
                    result.Items.ForEach(item =>
                    {
                        item.DepartmentName = list.Find(m => m.ID == item.DepartmentID)?.Name;
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
    }
}