using CabinetAPI.Controllers;
using CabinetData.Entities;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace CabinetAPI.Filter
{
    public class TokenFilterAttribute : ActionFilterAttribute
    {
        //匿名默认使用admin账户
        public static bool Anonymous = false;
        static TokenFilterAttribute()
        {
            Anonymous = (ConfigurationManager.AppSettings["Anonymous"]?.ToString().ToLower()=="true");
            if (Anonymous)
            {
                if (!UserController.LoginDictionary.ContainsKey("admin"))
                    UserController.LoginDictionary.Add("admin", UserInfo.GetOne("admin")); 
            }
        }
        private Logger _logger = LogManager.GetLogger("TokenFilterAttribute");
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.Request.RequestUri.AbsolutePath.ToLower().Equals("/api/user/login"))
            {
                try
                {
                    var token = new Controllers.BaseController().GetCookie("token");

                    if (string.IsNullOrEmpty(token))
                    {
                        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, new CommonResponse() { State = 2 });
                    }
                    else
                    {
                        if (!UserController.LoginDictionary.ContainsKey(token))
                        {
                            int id = 0;
                            if (!Int32.TryParse(token, out id))
                            {
                                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, new CommonResponse() { State = 2, Message = "Filter" });
                                return;
                            }
                            UserInfo user = UserInfo.GetOne(id);
                            if (user == null)
                            {
                                _logger.Info(token + "    历史数据：" + string.Join(",", UserController.LoginDictionary.Keys.ToList()));

                                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, new CommonResponse() { State = 2, Message = "Filter" });
                            }
                            else
                            {
                                UserController.LoginDictionary.Add(token, user);
                            }
                        }
                        //
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }
    }

    public class CommonResponse
    { 
        public int State { get; set; }
        public string Message { get; set; }

        public string Data { get; set; }
    }
}