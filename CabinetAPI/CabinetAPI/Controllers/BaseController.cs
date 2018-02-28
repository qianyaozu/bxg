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
        #region Action返回结果通用方法
        /// <summary>
        /// 返回成功数据
        /// </summary>
        /// <param name="data"></param>
        public JsonResult<dynamic> Success(object data)
        {
            var result = new
            {
                State = 1,
                Message = "",
                Data = data
            };
            return Json<dynamic>(result);
        }
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
        }

        public string GetCookie(string strName)
        {
            if (HttpContext.Current.Request.Cookies != null && HttpContext.Current.Request.Cookies[strName] != null)
            {
                return HttpContext.Current.Request.Cookies[strName].Value.ToString();
            }
            return "";
        }
        #endregion

        #region Session操作

        /// <summary>
        /// 写Session
        /// </summary>
        /// <typeparam name="T">Session键值的类型</typeparam>
        /// <param name="key">Session的键名</param>
        /// <param name="value">Session的键值</param>
        public void WriteSession<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
                return;
            HttpContext.Current.Session[key] = value;
        }

        /// <summary>
        /// 写Session
        /// </summary>
        /// <param name="key">Session的键名</param>
        /// <param name="value">Session的键值</param>
        public void WriteSession(string key, string value)
        {
            WriteSession<string>(key, value);
        }

        /// <summary>
        /// 读取Session的值
        /// </summary>
        /// <param name="key">Session的键名</param>        
        public object GetSession(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            return HttpContext.Current.Session[key];
        }
        

        /// <summary>
        /// 删除指定Session
        /// </summary>
        /// <param name="key">Session的键名</param>
        public void RemoveSession(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;
            HttpContext.Current.Session.Contents.Remove(key);
        }

        #endregion
    }
}