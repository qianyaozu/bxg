using System;
using System.Collections.Generic;
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
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.Request.RequestUri.AbsolutePath.ToLower().Equals("/api/user/login"))
            {
                try
                {
                    var token =  new Controllers.BaseController().GetCookie("token");
                    if (string.IsNullOrEmpty(token))
                    {
                        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, new CommonResponse() { State =2 });
                    }
                    else if (CabinetUtility.CacheHelper.GetCache(token)==null) 
                    {
                        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, new CommonResponse() { State = 2 });
                    }
                }
                catch (Exception ex)
                {

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