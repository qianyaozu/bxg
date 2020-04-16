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
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CabinetAPI.Controllers
{
    public class CommandResponse
    {
        public int ID { get; set; }
        public int OperationType { get; set; }
        public string AuthCode { get; set; }
        public DateTime CreateTime { get; set; }
    }
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
        public static List< CommandResponse> CommandDictionary = new List< CommandResponse>();
        public static object CommandDictionaryLock = new object();
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
        public async Task<IHttpActionResult> Upload(string mac)
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent("form-data"))
                    return Failure("必须选择图片");
                string root = System.Web.HttpContext.Current.Server.MapPath("/upload/");
                if (!Directory.Exists(root))
                    Directory.CreateDirectory(root);

                var provider = new RenamingMultipartFormDataStreamProvider(root, mac);//重命名写法
                await Request.Content.ReadAsMultipartAsync(provider);

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

        /// <summary>
        /// 请求命令接口
        ///正常开门 = 1,
        ///密码错误 = 2,
        ///正常关门 = 3,
        ///非工作时间开门 = 4,
        ///非工作时间关门 = 5,
        ///外部电源断开 = 6,
        ///备份电源电压低 = 7,
        ///未按规定关门 = 8,
        ///强烈震动 = 9,
        ///网络断开 = 10,
        ///修改密码 = 11,
        ///设置参数 = 12,
        ///上线 = 13,
        ///下线 = 14,
        ///请求语音 = 15,
        ///结束语音 = 16,
        ///心跳 = 17,
        ///接受语音 = 18,
        ///拒绝语音 = 19,
        ///允许开门 = 20,
        ///拒绝开门 = 21,
        ///申请维修 = 22,
        ///解除报警 = 23,
        ///申请开门 = 24,
        ///倾斜移动报警 = 30,
        ///上传门磁报警 = 31,
        ///终端修改信息 = 32,
        ///允许终端修改信息 = 33,
        ///拒绝终端修改信息 = 34,
        ///应急钥匙开门 = 35,
        ///申请人员注册 = 40,
        ///允许人员注册 = 41,
        ///拒绝人员注册 = 42,
        ///开门验证失败=50,
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

                #region 心跳
                if (request.OperatorType == (int)OperatorTypeEnum.心跳)
                {
                    if (HeartDictionary.ContainsKey(cabinet.ID))
                    {
                        HeartDictionary[cabinet.ID] = DateTime.Now;
                    }
                    else
                    {
                        HeartDictionary.TryAdd(cabinet.ID, DateTime.Now);
                    }
                    CommandResponse res=null;
                    int type = -1;
                    lock (CommandDictionaryLock)
                    {
                        var item = CommandDictionary.Find(m => m.ID == cabinet.ID);
                        if (item != null)
                        {
                            res = item;
                            CommandDictionary.Remove(item);
                        }
                    }
                   
                    if (res!=null)
                    {
                        type = res.OperationType;
                        var msg = "AuthCode-" + res.AuthCode;
                        
                        if (type == (int)OperatorTypeEnum.允许开门)
                        {
                            //cabinet.Status = (int)OperatorTypeEnum.允许开门;
                            //cabinet.Alarm = "";
                            //cabinet.IsOpen = true;
                            //Cabinet.Update(cabinet);
                            return Success1("允许开门", msg);
                        }
                        else if (type == (int)OperatorTypeEnum.拒绝开门)
                        {
                            return Success1("拒绝开门", msg);
                        }
                        else if (type == (int)OperatorTypeEnum.拒绝语音)
                        {
                            return Success1("拒绝语音", msg);
                        }
                        else if (type == (int)OperatorTypeEnum.接受语音)
                        {
                            return Success1("接受语音", msg);
                        }
                        else if (type == (int)OperatorTypeEnum.允许终端修改信息)
                        {
                            return Success1("允许终端修改信息", msg);
                        }
                        else if (type == (int)OperatorTypeEnum.拒绝终端修改信息)
                        {
                            return Success1("拒绝终端修改信息", msg);
                        }
                        else if (type == (int)OperatorTypeEnum.允许人员注册)
                        {
                            return Success1("允许人员注册", msg);
                        }
                        else if (type == (int)OperatorTypeEnum.拒绝人员注册)
                        {
                            return Success1("拒绝人员注册", msg);
                        }
                    }
                    return Success();
                }
                #endregion

                #region 增加请求日志
                var log = new CabinetLog
                {
                    CabinetID = cabinet.ID,
                    DepartmentID = cabinet.DepartmentID,
                    OperatorName = request.UserName,
                    OperateTime = DateTime.Now,
                    OperationType = request.OperatorType,
                    CreateTime = DateTime.Now,
                    CabinetIP = GetIP(),
                    EventContent = JsonConvert.SerializeObject(request),
                    Remark=request.AuthCode
                };
                CabinetLog.Add(log);

                #endregion
                #region 开门申请
                if (request.OperatorType == (int)OperatorTypeEnum.申请开门)
                {
                    if (!(cabinet.NeedConfirm ?? true))
                    {
                        lock (CommandDictionaryLock)
                        {
                            CommandDictionary.Add(new CommandResponse { OperationType = (int)OperatorTypeEnum.允许开门, AuthCode = request.AuthCode, ID = cabinet.ID, CreateTime = DateTime.Now });
                        }
                        request.OperatorType = (int)OperatorTypeEnum.允许开门;
                        request.EventContent = "不需要审核自动开门";
                        AddLog(cabinet, request);
                        return Success();
                    }
                    #region 密码开门
                    else if (request.Method == 0)
                    {
                        if (string.IsNullOrEmpty(request.Password))
                        {
                            CabinetLog.Add(new CabinetLog
                            {
                                CabinetID = cabinet.ID,
                                DepartmentID = cabinet.DepartmentID,
                                OperatorName = request.UserName,
                                OperateTime = DateTime.Now,
                                OperationType = (int)OperatorTypeEnum.密码错误,
                                CreateTime = DateTime.Now,
                                CabinetIP = GetIP(),
                                EventContent = JsonConvert.SerializeObject(request)
                            });
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
                                OperationType = (int)OperatorTypeEnum.密码错误,
                                CreateTime = DateTime.Now,
                                CabinetIP = GetIP(),
                                EventContent = JsonConvert.SerializeObject(request)
                            });
                            _loggerError.Error("密码错误 " + request.Password + "  [" + cabinet.FirstContactPassword + ":" + first + "]  [" + cabinet.SecondContactPassword + ":" + second + "]");
                            return Failure("密码错误");
                        }
                    }
                    #endregion

                    #region 加入前台提醒
                    lock (logLock)
                    {
                        msgID++;
                        log.ID = msgID;
                        CabinetLogQueue.Add(log);
                    }
                    return Success("等待审核");
                    #endregion
                }

                #endregion

                #region 语音申请||终端修改信息||申请人员注册
                else if (request.OperatorType == (int)OperatorTypeEnum.请求语音 || request.OperatorType == (int)OperatorTypeEnum.终端修改信息 || request.OperatorType == (int)OperatorTypeEnum.申请人员注册)
                {
                    lock (logLock)
                    {
                        msgID++;
                        log.ID = msgID;
                        CabinetLogQueue.Add(log);
                    }
                    return Success("等待审核");
                }
                #endregion


                #region 关门确认
                else if (request.OperatorType == (int)OperatorTypeEnum.正常关门)
                {
                    if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 17 || DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                    {
                        cabinet.Status = (int)OperatorTypeEnum.非工作时间关门;
                        cabinet.Alarm = OperatorTypeEnum.非工作时间关门.ToString();
                    }
                    else
                    {
                        cabinet.Status = (int)OperatorTypeEnum.正常关门;
                        cabinet.Alarm = "";

                    }
                    cabinet.IsOpen = false;
                    Cabinet.Update(cabinet);
                }
                #endregion

                #region 开门确认
                else if (request.OperatorType == (int)OperatorTypeEnum.正常开门)
                {
                    if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 17 || DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                    {
                        cabinet.Status = (int)OperatorTypeEnum.非工作时间开门;
                        cabinet.Alarm = OperatorTypeEnum.非工作时间开门.ToString();
                    }
                    else
                    {
                        cabinet.Status = (int)OperatorTypeEnum.正常开门;
                        cabinet.Alarm = "";

                    }
                    cabinet.IsOpen = true;
                    Cabinet.Update(cabinet);
                }
                #endregion

                #region 应急钥匙开门
                else if (request.OperatorType == (int)OperatorTypeEnum.应急钥匙开门)
                {
                    cabinet.Status = (int)OperatorTypeEnum.应急钥匙开门;
                    cabinet.Alarm = OperatorTypeEnum.应急钥匙开门.ToString();
                    cabinet.IsOpen = true;
                    Cabinet.Update(cabinet);
                }
                #endregion

                #region 其他请求
                else
                {
                    cabinet.Status = request.OperatorType;
                    cabinet.Alarm = Enum.GetName(typeof(OperatorTypeEnum), cabinet.Status);
                    Cabinet.Update(cabinet);
                }
                #endregion

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

        private void AddLog(Cabinet cabinet, CommandRequest request)
        {
            var log = new CabinetLog
            {
                CabinetID = cabinet.ID,
                DepartmentID = cabinet.DepartmentID,
                OperatorName = request.UserName,
                OperateTime = DateTime.Now,
                OperationType = request.OperatorType,
                CreateTime = DateTime.Now,
                CabinetIP = GetIP(),
                EventContent = JsonConvert.SerializeObject(request)
            };
            CabinetLog.Add(log);
        }



        /// <summary>
        /// 请求命令接口
        ///正常开门 = 1,
        ///密码错误 = 2,
        ///正常关门 = 3,
        ///非工作时间开门 = 4,
        ///非工作时间关门 = 5,
        ///外部电源断开 = 6,
        ///备份电源电压低 = 7,
        ///未按规定关门 = 8,
       ///强烈震动 = 9,
        ///网络断开 = 10,
        ///修改密码 = 11,
        ///设置参数 = 12,
        ///上线 = 13,
        ///下线 = 14,
        ///请求语音 = 15,
        ///结束语音 = 16,
        ///心跳 = 17,
        ///接受语音 = 18,
        ///拒绝语音 = 19,
        ///允许开门 = 20,
        ///拒绝开门 = 21,
        ///申请维修 = 22,
        ///解除报警 = 23,
        ///申请开门 = 24,
        ///倾斜移动报警 = 30,
        ///上传门磁报警 = 31,
        ///终端修改信息 = 32,
        ///允许终端修改信息 = 33,
        ///拒绝终端修改信息 = 34,
        ///应急钥匙开门 = 35,
        ///申请人员注册 = 40,
        ///允许人员注册 = 41,
        ///拒绝人员注册 = 42,
        ///开门验证失败=50,
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost, Route("api/android/Command1")]
        public IHttpActionResult Command1(CommandRequest request)
        {
            //if (request == null)
            //    return Failure("请求命令无效");
            //if (string.IsNullOrEmpty(request.Mac))
            //    return Failure("必须指定mac地址");
            //try
            //{
            //    _logger.Info(JsonConvert.SerializeObject(request));
            //    var cabinet = Cabinet.GetByMac(request.Mac);
            //    if (cabinet == null)
            //        return Failure("未找到指定保险柜");


            //    if (request.OperatorType == (int)OperatorTypeEnum.正常开门)
            //    {
            //        #region 申请 正常开门
            //        if (request.Method == 0)
            //        {
            //            if (string.IsNullOrEmpty(request.Password))
            //            {
            //                CabinetLog.Add(new CabinetLog
            //                {
            //                    CabinetID = cabinet.ID,
            //                    DepartmentID = cabinet.DepartmentID,
            //                    OperatorName = request.UserName,
            //                    OperateTime = DateTime.Now,
            //                    OperationType = (int)OperatorTypeEnum.密码错误,
            //                    CreateTime = DateTime.Now,
            //                    CabinetIP = GetIP(),
            //                    EventContent = "密码不得为空"
            //                });
            //                _loggerError.Error("密码不得为空");
            //                return Failure("密码不得为空");
            //            }
            //            var first = string.IsNullOrEmpty(cabinet.FirstContactPassword) ? "" : AESAlgorithm.Decrypto(cabinet.FirstContactPassword);
            //            var second = string.IsNullOrEmpty(cabinet.SecondContactPassword) ? "" : AESAlgorithm.Decrypto(cabinet.SecondContactPassword);
            //            if (first != request.Password && second != request.Password)
            //            {
            //                CabinetLog.Add(new CabinetLog
            //                {
            //                    CabinetID = cabinet.ID,
            //                    DepartmentID = cabinet.DepartmentID,
            //                    OperatorName = request.UserName,
            //                    OperateTime = DateTime.Now,
            //                    OperationType = (int)OperatorTypeEnum.密码错误,
            //                    CreateTime = DateTime.Now,
            //                    CabinetIP = GetIP(),
            //                    EventContent = "密码错误"
            //                });
            //                _loggerError.Error("密码错误 " + request.Password + "  [" + cabinet.FirstContactPassword + ":" + first + "]  [" + cabinet.SecondContactPassword + ":" + second + "]");
            //                return Failure("密码错误");
            //            }
            //        }else
            //        {
            //            if (!(cabinet.NeedConfirm ?? true))
            //            {
            //                //不需要验证
            //                //return Success("允许开门");
            //            }
            //        }
            //        #endregion
            //    }
            //    else if (request.OperatorType == (int)OperatorTypeEnum.正常关门)
            //    {
            //        if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 17 || DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            //        {
            //            request.OperatorType = (int)OperatorTypeEnum.非工作时间关门;
            //        }
            //        //cabinet.Status = request.OperatorType;
            //        //Cabinet.Update(cabinet);
            //    }
            //    else if (request.OperatorType == (int)OperatorTypeEnum.心跳)
            //    {

            //        //if (cabinet.IsOnline ?? false)
            //        //{
            //        //    var log1 = new CabinetData.Entities.CabinetLog
            //        //    {
            //        //        CabinetID = cabinet.ID,
            //        //        DepartmentID = cabinet.DepartmentID,
            //        //        OperatorName = "",
            //        //        OperateTime = DateTime.Now,
            //        //        OperationType = (int)OperatorTypeEnum.上线,
            //        //        CreateTime = DateTime.Now,
            //        //        CabinetIP = "",
            //        //        EventContent = ""
            //        //    };
            //        //    CabinetLog.Add(log1);
            //        //    cabinet.IsOnline = true;
            //        //    Cabinet.Update(cabinet);
            //        //    lock (AndroidController.logLock)
            //        //        AndroidController.CabinetLogQueue.Add(log1);
            //        //}


            //        if (HeartDictionary.ContainsKey(cabinet.ID))
            //        {
            //            HeartDictionary[cabinet.ID] = DateTime.Now;
            //        }
            //        else
            //        {
            //            HeartDictionary.TryAdd(cabinet.ID, DateTime.Now);
            //        }
            //        if (CommandDictionary.ContainsKey(cabinet.ID))
            //        {
            //            CommandResponse res;
            //            int type = -1;
            //            if (CommandDictionary.TryRemove(cabinet.ID, out res))
            //            {
            //                type = res.OperationType;
            //                if (type == (int)OperatorTypeEnum.允许开门)
            //                {
            //                    if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 17 || DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            //                        request.OperatorType = (int)OperatorTypeEnum.非工作时间开门;
            //                    else
            //                        request.OperatorType = (int)OperatorTypeEnum.正常开门;
            //                    cabinet.Status = request.OperatorType;
            //                    Cabinet.Update(cabinet);
            //                    var log1 = new CabinetLog
            //                    {
            //                        CabinetID = cabinet.ID,
            //                        DepartmentID = cabinet.DepartmentID,
            //                        OperatorName = request.UserName,
            //                        OperateTime = DateTime.Now,
            //                        OperationType = (int)OperatorTypeEnum.允许开门,
            //                        CreateTime = DateTime.Now,
            //                        CabinetIP = GetIP(),
            //                        EventContent = ""
            //                    };
            //                    CabinetLog.Add(log1);
            //                    return Success("允许开门");
            //                }
            //                else if (type == (int)OperatorTypeEnum.拒绝开门)
            //                {
            //                    var log1 = new CabinetLog
            //                    {
            //                        CabinetID = cabinet.ID,
            //                        DepartmentID = cabinet.DepartmentID,
            //                        OperatorName = request.UserName,
            //                        OperateTime = DateTime.Now,
            //                        OperationType = (int)OperatorTypeEnum.拒绝开门,
            //                        CreateTime = DateTime.Now,
            //                        CabinetIP = GetIP(),
            //                        EventContent = ""
            //                    };
            //                    CabinetLog.Add(log1);
            //                    return Success("拒绝开门");
            //                }
            //                else if (type == (int)OperatorTypeEnum.拒绝语音)
            //                {
            //                    return Success("拒绝语音");
            //                }
            //                else if (type == (int)OperatorTypeEnum.接受语音)
            //                {
            //                    return Success("接受语音");
            //                }
            //                else if (type == (int)OperatorTypeEnum.允许终端修改信息)
            //                {
            //                    return Success("允许终端修改信息");
            //                }
            //                else if (type == (int)OperatorTypeEnum.拒绝终端修改信息)
            //                {
            //                    return Success("拒绝终端修改信息");
            //                }
            //            }
            //        }
            //        return Success();
            //    }
            //    //正常开门 = 1,
            //    //密码错误 = 2,
            //    //正常关门 = 3,
            //    //非工作时间开门 = 4,
            //    //非工作时间关门 = 5,
            //    //外部电源断开 = 6,
            //    //备份电源电压低 = 7,
            //    //未按规定关门 = 8,
            //    //强烈震动 = 9,
            //    //网络断开 = 10, 
            //    //请求语音 = 15,
            //    //结束语音 = 16,
            //    //申请维修 = 22
            //    // 倾斜移动报警 =30
            //    // 上传门磁报警 =31
            //    List<int> statusList = new List<int>() { 3, 4, 5, 6, 7, 8, 9, 10,30,31 };
            //    if (statusList.Contains(request.OperatorType))
            //    {
            //        cabinet.Status = request.OperatorType;
            //        Cabinet.Update(cabinet);
            //    }
            //    var log = new CabinetLog
            //    {
            //        CabinetID = cabinet.ID,
            //        DepartmentID = cabinet.DepartmentID,
            //        OperatorName = request.UserName,
            //        OperateTime = DateTime.Now,
            //        OperationType = request.OperatorType,
            //        CreateTime = DateTime.Now,
            //        CabinetIP = GetIP(),
            //        EventContent = (request.OperatorType == (int)OperatorTypeEnum.正常开门 || request.OperatorType == (int)OperatorTypeEnum.非工作时间开门) ? JsonConvert.SerializeObject(request) : request.EventContent
            //    };
            //    CabinetLog.Add(log);
            //    if ((request.OperatorType == (int)OperatorTypeEnum.正常开门 || request.OperatorType == (int)OperatorTypeEnum.非工作时间开门))
            //    {
            //        if (!(cabinet.NeedConfirm ?? true))
            //        {
            //            //不要验证
            //            if (CommandDictionary.ContainsKey(cabinet.ID))
            //            {
            //                CommandDictionary[cabinet.ID].OperationType = (int)OperatorTypeEnum.允许开门;
            //            }
            //            else
            //            {
            //                CommandDictionary.TryAdd(cabinet.ID, new CommandResponse { OperationType = (int)OperatorTypeEnum.允许开门 , AuthCode = request.AuthCode });
            //            }
            //            if (DateTime.Now.Hour < 9 || DateTime.Now.Hour > 17 || DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            //                request.OperatorType = (int)OperatorTypeEnum.非工作时间开门;
            //            else
            //                request.OperatorType = (int)OperatorTypeEnum.正常开门;
            //            cabinet.Status = request.OperatorType;
            //            Cabinet.Update(cabinet);
            //            var log1 = new CabinetLog
            //            {
            //                CabinetID = cabinet.ID,
            //                DepartmentID = cabinet.DepartmentID,
            //                OperatorName = request.UserName,
            //                OperateTime = DateTime.Now,
            //                OperationType = (int)OperatorTypeEnum.允许开门,
            //                CreateTime = DateTime.Now,
            //                CabinetIP = GetIP(),
            //                EventContent = "不需要审核自动开门"
            //            };
            //            CabinetLog.Add(log1);

            //            return Success();
            //        }
            //        else
            //        {
            //            lock (logLock)
            //            {
            //                msgID++;
            //                log.ID = msgID;
            //                CabinetLogQueue.Add(log);
            //            }
            //            return Success("等待审核");
            //        }
            //    }
            //    if (request.OperatorType == (int)OperatorTypeEnum.请求语音|| request.OperatorType == (int)OperatorTypeEnum.终端修改信息)
            //    {
            //        lock (logLock)
            //        {
            //            msgID++;
            //            log.ID = msgID;
            //            CabinetLogQueue.Add(log);
            //        }
            //        return Success("等待审核");
            //    }
            //    lock (logLock)
            //    {
            //        msgID++;
            //        log.ID = msgID;
            //        CabinetLogQueue.Add(log);
            //    }
            //    return Success();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex);
            //    return Failure("上报失败");
            //}
            return Failure("");
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