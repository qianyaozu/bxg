using CabinetData.Entities;
using CabinetData.Entities.Principal;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace CabinetAPI.Controllers
{
    public class AndroidController : BaseController
    {
        public static ConcurrentDictionary<int, DateTime> HeartDictionary = new ConcurrentDictionary<int, DateTime>();
        public static List<CabinetLog> CabinetLogQueue = new List<CabinetLog>();
        public static object logLock = new object();

        /// <summary>
        /// 对时接口
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("api/android/CheckTime")]
        public IHttpActionResult CheckTime()
        {
            return Success(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// 对版本接口
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [HttpGet, Route("api/android/CheckVersion")]
        public IHttpActionResult CheckVersion(string mac, string version)
        {
            if (string.IsNullOrEmpty(mac) || string.IsNullOrEmpty(version))
                return Failure("mac or version can't be empty");
            var cabinet = Cabinet.GetByMac(mac);
            if (cabinet == null)
                return Failure("this cabinet is not register");
            if (string.IsNullOrEmpty(cabinet.AndroidVersion))
            {
                cabinet.AndroidVersion = version;
                Cabinet.Update(cabinet);
            }
            if (version != cabinet.AndroidVersion)
            {
                return Success("/files/" + cabinet.AndroidVersion + ".apk");
            }
            else
            {
                return Success("");
            }
        }

        /// <summary>
        /// 上传图片接口
        /// </summary>
        /// <param name="mac"></param>
        /// <returns></returns>
        [HttpPost, Route("api/android/Upload")]
        public IHttpActionResult Upload(string mac)
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent("form-data"))
                    return Failure("必须选择图片");
                string root = System.Web.HttpContext.Current.Server.MapPath("/upload/");
                if (!Directory.Exists(root))
                    Directory.CreateDirectory(root);
                var provider = new RenamingMultipartFormDataStreamProvider(root, mac);//重命名写法
                Request.Content.ReadAsMultipartAsync(provider);
                if (provider == null || provider.FileData.Count == 0)
                    return Failure("上传失败");
                return Success(provider.FileData[0].LocalFileName);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("上传失败");

            }
        }


        //[HttpPost, Route("api/android/Open")]
        //public IHttpActionResult Open(OpenRequest request)
        //{
        //    if (request == null)
        //        return Failure("请求命令无效");
        //    if (string.IsNullOrEmpty(request.Mac))
        //        return Failure("必须指定mac地址");
        //    var cabinet = Cabinet.GetByMac(request.Mac);
        //    if (cabinet == null)
        //        return Failure("未找到指定保险柜");
        //    //if (cabinet.IP != GetIP())
        //    //    return Failure("IP校验失败");
        //    //if (cabinet.FirstContact != request.UserName && cabinet.SecondContact != request.UserName)
        //    //    return Failure("用户验证不匹配");
        //    //if (request.Method == 0)
        //    //{
        //    //    //校验密码
        //    //    if (string.IsNullOrEmpty(request.Password))
        //    //        return Failure("密码不得为空");
        //    //    if (cabinet.FirstContactPassword != request.Password && cabinet.SecondContactPassword != request.Password)
        //    //    {
        //    //        return Failure("密码错误");
        //    //    }
        //    //}

        //    int openType = (int)OperatorTypeEnum.正常开门;
        //    if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 17 || DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
        //        openType = (int)OperatorTypeEnum.非工作时间开门;
        //    CabinetLog.Add(new CabinetLog
        //    {
        //        CabinetID = cabinet.ID,
        //        OperatorName = request.UserName,
        //        OperateTime = DateTime.Now,
        //        OperationType = openType,
        //        CreateTime = DateTime.Now,
        //        CabinetIP = GetIP(),
        //        EventContent = JsonConvert.SerializeObject(request)
        //    });
        //    return Success("允许开门");
        //}



        /// <summary>
        /// 请求命令接口
        /// 正常开门 = 1,
        /// 密码错误 = 2,
        /// 正常关门 = 3,
        /// 非工作时间开门 = 4,
        /// 非工作时间关门 = 5,
        /// 外部电源断开 = 6,
        /// 备份电源电压低 = 7,
        /// 未按规定关门 = 8,
        /// 强烈震动 = 9,
        /// 网络断开 = 10, 
        /// 请求语音 = 15,
        /// 结束语音 = 16,
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost, Route("api/android/Command")]
        public IHttpActionResult Command(CommandRequest request)
        {
            if (request == null)
                return Failure("请求命令无效");
            if (string.IsNullOrEmpty(request.Mac))
                return Failure("必须指定mac地址");
            try
            {
                var cabinet = Cabinet.GetByMac(request.Mac);
                if (cabinet == null)
                    return Failure("未找到指定保险柜");
                if (request.OperatorType == (int)OperatorTypeEnum.正常开门)
                {
                    if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 17 || DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                        request.OperatorType = (int)OperatorTypeEnum.非工作时间开门;
                }
                else if (request.OperatorType == (int)OperatorTypeEnum.正常关门)
                {
                    if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 17 || DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                    {

                        request.OperatorType = (int)OperatorTypeEnum.非工作时间关门;
                    }
                }
                else if (request.OperatorType == (int)OperatorTypeEnum.心跳)
                {
                    if (HeartDictionary.ContainsKey(cabinet.ID))
                    {
                        HeartDictionary[cabinet.ID] = DateTime.Now;
                    }
                    else
                    {
                        HeartDictionary.TryAdd(cabinet.ID, DateTime.Now);
                    }
                }
                //正常开门 = 1,
                //密码错误 = 2,
                //正常关门 = 3,
                //非工作时间开门 = 4,
                //非工作时间关门 = 5,
                //外部电源断开 = 6,
                //备份电源电压低 = 7,
                //未按规定关门 = 8,
                //强烈震动 = 9,
                //网络断开 = 10, 
                //请求语音 = 15,
                //结束语音 = 16,
                List<int> statusList = new List<int>() { 1, 3, 4, 5, 6, 7, 8, 9, 10 };
                if (statusList.Contains(request.OperatorType))
                {
                    cabinet.Status = request.OperatorType;
                    Cabinet.Update(cabinet);
                }
                var log = new CabinetLog
                {
                    CabinetID = cabinet.ID,
                    DepartmentID = cabinet.DepartmentID,
                    OperatorName = request.UserName,
                    OperateTime = DateTime.Now,
                    OperationType = request.OperatorType,
                    CreateTime = DateTime.Now,
                    CabinetIP = GetIP(),
                    EventContent = (request.OperatorType == (int)OperatorTypeEnum.正常开门) ? JsonConvert.SerializeObject(request) : request.EventContent
                };
                CabinetLog.Add(log);
                lock (logLock)
                    CabinetLogQueue.Add(log);
                
                if (request.OperatorType == (int)OperatorTypeEnum.请求语音)
                {
                    return Success("链接语音服务器");
                }
                return Success("success");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("上报失败");
            }
        }

       
    }


    public class CommandRequest
    {
        public string Mac { get; set; }
        public string UserName { get; set; }

        /// <summary>
        /// 命令类型
        /// </summary>
        public int OperatorType { get; set; }

        /// <summary>
        /// 日志详细信息
        /// </summary>
        public string EventContent { get; set; }


        /// 开门命令参数

        public string Password { get; set; }//密码
        public int Method { get; set; } //0为密码，1为指纹
        public List<string> Photos { get; set; }//照片url

        public List<string> FingerPrint { get; set; } //指纹url

    }

    public class RenamingMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public string Root { get; set; }
        public string Mac { get; set; }

        public RenamingMultipartFormDataStreamProvider(string root, string mac)
            : base(root)
        {
            Root = root;
            Mac = mac;
        }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            var extenssion = headers.ContentDisposition.FileName;

            if (extenssion.StartsWith(@"""") && extenssion.EndsWith(@""""))
                extenssion = extenssion.Substring(1, extenssion.Length - 2);
            extenssion = Path.GetExtension(extenssion);
            string filePath = Mac + "_" + Guid.NewGuid().ToString() + "." + extenssion;

            return Path.GetFileName(filePath);
        }

    }
}