using CabinetAPI.Filter;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace CabinetAPI.Controllers
{
    public class BaseController : ApiController
    {
        public Logger logger = LogManager.GetCurrentClassLogger();
        #region Action返回结果通用方法
        [HiddenApi]
        /// <summary>
        /// 返回成功数据
        /// </summary>
        /// <param name="data"></param>
        public JsonResult<dynamic> Success(object data=null)
        {
            var result = new
            {
                State = 1,
                Message = "",
                Data = data
            };
            return Json<dynamic>(result);
        }
        [HiddenApi]
        /// <summary>
        /// 返回失败数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        public JsonResult<dynamic> Failure(string message, object data = null)
        {
            var result = new
            {
                State = 0,
                Message = message,
                Data = data
            };
            return Json<dynamic>(result);
        }


        public JsonResult<dynamic> Logout()
        {
            var result = new
            {
                State = 2,
            };
            return Json<dynamic>(result);
        }
        #endregion

        #region 读写Cookie
        public void WriteCookie(string strName, string strValue, int expires = 0)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[strName];
            if (cookie == null)
            {
                cookie = new HttpCookie(strName);
            }
            cookie.Value = strValue;
            if (expires != 0)
                cookie.Expires = DateTime.Now.AddMinutes(expires);
            HttpContext.Current.Response.AppendCookie(cookie);
            HttpContext.Current.Response.AddHeader(strName, strValue);
        }

        public string GetCookie(string strName)
        {
            if (HttpContext.Current.Request.Cookies != null && HttpContext.Current.Request.Cookies[strName] != null)
            {
                return HttpContext.Current.Request.Cookies[strName].Value.ToString();
            }      
            return HttpContext.Current.Request.Headers[strName] ?? "";
        }
        #endregion

        

        #region 获取请求IP
        public string GetIP()
        {
            string result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(result))
            {
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (string.IsNullOrEmpty(result))
            {
                result = HttpContext.Current.Request.UserHostAddress;
            }
            if (string.IsNullOrEmpty(result))
            {
                return "0.0.0.0";
            }
            return result;
        }
        #endregion
    }
}