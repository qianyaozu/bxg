using CabinetAPI.Models;
using CabinetData.Entities;
using CabinetData.Entities.Principal;
using CabinetService.Model;
using CabinetUtility.Encryption;
using Newtonsoft.Json;
using NLog;
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
        private Logger _logger = LogManager.GetLogger("消息队列");
        private Logger _loggerError = LogManager.GetLogger("error");
        /// <summary>
        /// 心跳集合
        /// </summary>
        public static ConcurrentDictionary<int, DateTime> HeartDictionary = new ConcurrentDictionary<int, DateTime>();
        /// <summary>
        /// 命令集合
        /// </summary>
        public static ConcurrentDictionary<int, int> CommandDictionary = new ConcurrentDictionary<int, int>();
        public static List<CabinetLog> CabinetLogQueue = new List<CabinetLog>();
        public static object logLock = new object();
        public static int msgID = 0;
        static AndroidController()
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            msgID = (int)(DateTime.Now - startTime).TotalSeconds; // 相差毫秒数
        }

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
                FileInfo info = new FileInfo(provider.FileData[0].LocalFileName);
                if (info.Exists)
                {
                    return Success(info.Name);
                }
                else
                {
                    return Success("");
                }
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
        /// 申请维修 = 22
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
                _logger.Info(JsonConvert.SerializeObject(request));
                var cabinet = Cabinet.GetByMac(request.Mac);
                if (cabinet == null)
                    return Failure("未找到指定保险柜");
                if (request.OperatorType == (int)OperatorTypeEnum.正常开门)
                {
                    if (request.Method == 0)
                    {
                        if (string.IsNullOrEmpty(request.Password))
                        {
                            _loggerError.Error("密码不得为空");
                            return Failure("密码不得为空");
                        }
                        var first = string.IsNullOrEmpty(cabinet.FirstContactPassword) ? "" : AESAlgorithm.Decrypto(cabinet.FirstContactPassword);
                        var second = string.IsNullOrEmpty(cabinet.SecondContactPassword) ? "" : AESAlgorithm.Decrypto(cabinet.SecondContactPassword);
                        if (first != request.Password && second != request.Password)
                        {
                            CabinetLog.Add(new CabinetLog
                            {
                                CabinetID = cabinet.ID,
                                DepartmentID = cabinet.DepartmentID,
                                OperatorName = request.UserName,
                                OperateTime = DateTime.Now,
                                OperationType = request.OperatorType,
                                CreateTime = DateTime.Now,
                                CabinetIP = GetIP(),
                                EventContent = "密码错误"
                            });
                            _loggerError.Error("密码错误 " + request.Password + "  [" + cabinet.FirstContactPassword + ":" + first + "]  [" + cabinet.SecondContactPassword + ":" + second + "]");
                            return Failure("密码错误");
                        }
                    }else
                    {
                        if (!(cabinet.NeedConfirm ?? true))
                        {
                            //不需要验证
                            //return Success("允许开门");
                        }
                    }
                }
                else if (request.OperatorType == (int)OperatorTypeEnum.正常关门)
                {
                    if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 17 || DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                    {
                        request.OperatorType = (int)OperatorTypeEnum.非工作时间关门;
                    }
                    //cabinet.Status = request.OperatorType;
                    //Cabinet.Update(cabinet);
                }
                else if (request.OperatorType == (int)OperatorTypeEnum.心跳)
                {

                    //if (cabinet.IsOnline ?? false)
                    //{
                    //    var log1 = new CabinetData.Entities.CabinetLog
                    //    {
                    //        CabinetID = cabinet.ID,
                    //        DepartmentID = cabinet.DepartmentID,
                    //        OperatorName = "",
                    //        OperateTime = DateTime.Now,
                    //        OperationType = (int)OperatorTypeEnum.上线,
                    //        CreateTime = DateTime.Now,
                    //        CabinetIP = "",
                    //        EventContent = ""
                    //    };
                    //    CabinetLog.Add(log1);
                    //    cabinet.IsOnline = true;
                    //    Cabinet.Update(cabinet);
                    //    lock (AndroidController.logLock)
                    //        AndroidController.CabinetLogQueue.Add(log1);
                    //}


                    if (HeartDictionary.ContainsKey(cabinet.ID))
                    {
                        HeartDictionary[cabinet.ID] = DateTime.Now;
                    }
                    else
                    {
                        HeartDictionary.TryAdd(cabinet.ID, DateTime.Now);
                    }
                    if (CommandDictionary.ContainsKey(cabinet.ID))
                    {
                        int type = -1;
                        if (CommandDictionary.TryRemove(cabinet.ID, out type))
                        {
                            if (type == (int)OperatorTypeEnum.允许开门)
                            {
                                if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 17 || DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                                    request.OperatorType = (int)OperatorTypeEnum.非工作时间开门;
                                else
                                    request.OperatorType = (int)OperatorTypeEnum.正常开门;
                                cabinet.Status = request.OperatorType;
                                Cabinet.Update(cabinet);
                                var log1 = new CabinetLog
                                {
                                    CabinetID = cabinet.ID,
                                    DepartmentID = cabinet.DepartmentID,
                                    OperatorName = request.UserName,
                                    OperateTime = DateTime.Now,
                                    OperationType = (int)OperatorTypeEnum.允许开门,
                                    CreateTime = DateTime.Now,
                                    CabinetIP = GetIP(),
                                    EventContent = ""
                                };
                                CabinetLog.Add(log1);
                                return Success("允许开门");
                            }
                            else if (type == (int)OperatorTypeEnum.拒绝开门)
                            {
                                var log1 = new CabinetLog
                                {
                                    CabinetID = cabinet.ID,
                                    DepartmentID = cabinet.DepartmentID,
                                    OperatorName = request.UserName,
                                    OperateTime = DateTime.Now,
                                    OperationType = (int)OperatorTypeEnum.拒绝开门,
                                    CreateTime = DateTime.Now,
                                    CabinetIP = GetIP(),
                                    EventContent = ""
                                };
                                CabinetLog.Add(log1);
                                return Success("拒绝开门");
                            }
                            else if (type == (int)OperatorTypeEnum.拒绝语音)
                            {
                                return Success("拒绝语音");
                            }
                            else if (type == (int)OperatorTypeEnum.接受语音)
                            {
                                return Success("接受语音");
                            }
                        }
                    }
                    return Success();
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
                //申请维修 = 22
                List<int> statusList = new List<int>() { 3, 4, 5, 6, 7, 8, 9, 10 };
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
                    EventContent = (request.OperatorType == (int)OperatorTypeEnum.正常开门 || request.OperatorType == (int)OperatorTypeEnum.非工作时间开门) ? JsonConvert.SerializeObject(request) : request.EventContent
                };
                CabinetLog.Add(log);
                if ((request.OperatorType == (int)OperatorTypeEnum.正常开门 || request.OperatorType == (int)OperatorTypeEnum.非工作时间开门))
                {
                    if (!(cabinet.NeedConfirm ?? true))
                    {
                        //不要验证
                        if (CommandDictionary.ContainsKey(cabinet.ID))
                        {
                            CommandDictionary[cabinet.ID] = (int)OperatorTypeEnum.允许开门;
                        }
                        else
                        {
                            CommandDictionary.TryAdd(cabinet.ID, (int)OperatorTypeEnum.允许开门);
                        }
                        if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 17 || DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                            request.OperatorType = (int)OperatorTypeEnum.非工作时间开门;
                        else
                            request.OperatorType = (int)OperatorTypeEnum.正常开门;
                        cabinet.Status = request.OperatorType;
                        Cabinet.Update(cabinet);
                        var log1 = new CabinetLog
                        {
                            CabinetID = cabinet.ID,
                            DepartmentID = cabinet.DepartmentID,
                            OperatorName = request.UserName,
                            OperateTime = DateTime.Now,
                            OperationType = (int)OperatorTypeEnum.允许开门,
                            CreateTime = DateTime.Now,
                            CabinetIP = GetIP(),
                            EventContent = ""
                        };
                        CabinetLog.Add(log1);

                        return Success();
                    }
                    else
                    {
                        lock (logLock)
                        {
                            msgID++;
                            log.ID = msgID;
                            CabinetLogQueue.Add(log);
                        }
                        return Success("等待审核");
                    }
                }
                if (request.OperatorType == (int)OperatorTypeEnum.请求语音)
                {
                    lock (logLock)
                    {
                        msgID++;
                        log.ID = msgID;
                        CabinetLogQueue.Add(log);
                    }
                    return Success("等待接受");
                }
                lock (logLock)
                {
                    msgID++;
                    log.ID = msgID;
                    CabinetLogQueue.Add(log);
                }
                return Success();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("上报失败");
            }
        }



        [HttpGet, Route("api/getall")]
        public IHttpActionResult GetAll()
        {
            try
            {
                var cabinetList = Cabinet.GetAll().FindAll(m => (m.Status == 1 || m.Status == 4));
                foreach (var m in cabinetList)
                {
                    var log = CabinetLog.GetOpenLog(m.ID);
                    if (log == null || log.CreateTime.AddSeconds(60) < DateTime.Now)
                    {
                        m.Status = 3;
                        Cabinet.Update(m);
                        _logger.Info("自动重置关闭");
                    }
                }
                var list = Cabinet.GetAll();
                var model = list.Select(m => new BxgModel
                {
                    Name = m.Name,
                    Mac = m.AndroidMac,
                    IP = m.IP,
                    IsOnline = m.IsOnline ?? false,
                    LastOnlineTime = (m.LastOnlineTime == null) ? "" : m.LastOnlineTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                    //正常开门 = 1, 
                    //正常关门 = 3,
                    //非工作时间开门 = 4,
                    //非工作时间关门 = 5,
                    //外部电源断开 = 6,
                    //备份电源电压低 = 7,
                    //未按规定关门 = 8,
                    //强烈震动 = 9,
                    //网络断开 = 10, 
                    Status = (m.Status == 1 || m.Status == 4) ? 1 : 0,
                    StatusDes = (m.Status == null ? "" : Enum.GetName(typeof(OperatorTypeEnum), m.Status)),
                }).ToList();
                var item= new ResultModel()
                {
                    Success = 1,
                    Message = "",
                    Data = model
                };
                return Json<ResultModel>(item);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                var item = new ResultModel()
                {
                    Success = 0,
                    Message = "获取数据失败",
                    Data = null
                };
                return Json<ResultModel>(item); 
            }
        }
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
            //var extenssion = headers.ContentDisposition.FileName;

            //if (extenssion.StartsWith(@"""") && extenssion.EndsWith(@""""))
            //    extenssion = extenssion.Substring(1, extenssion.Length - 2);
            //extenssion = Path.GetExtension(extenssion);
            return Mac + "_" + Guid.NewGuid().ToString() + ".png";
        }

    }
}