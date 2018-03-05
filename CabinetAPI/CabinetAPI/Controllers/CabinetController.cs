using CabinetData.Entities;
using CabinetData.Entities.QueryEntities;
using CabinetUtility.Encryption;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CabinetAPI.Controllers
{
    public class CabinetController : BaseController
    {

        

        [HttpPost, Route("api/cabinet/add")]
        public IHttpActionResult AddCabinet(Cabinet cabinet)
        {
            try
            {
                string valiate = ValiateCabinetModel(cabinet);
                if (!string.IsNullOrEmpty(valiate))
                {
                    return Failure(valiate);
                }
                cabinet.CreateTime = DateTime.Now;
                cabinet.IsOnline = false;
                Cabinet.Add(cabinet);
                return Success(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("新增失败");
            }

        }

        /// <summary>
        /// 验证用户实体类
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string ValiateCabinetModel(Cabinet cabinet)
        {
            if (cabinet == null)
                return "参数错误";
            if (cabinet.DepartmentID == 0)
                return "请指定部门";
            if (string.IsNullOrEmpty(cabinet.Address))
                return "请设置所在地址";
            if (string.IsNullOrEmpty(cabinet.Name))
                return "请设置保险柜名称";
            if (string.IsNullOrEmpty(cabinet.Code))
                return "请设置保险柜编号";
            if (string.IsNullOrEmpty(cabinet.AndroidMac))
                return "请设置Android硬件地址";
            if (string.IsNullOrEmpty(cabinet.IP))
                return "请设置分配IP";
            return null;
        }

        [HttpPost, Route("api/cabinet/edit")]
        public IHttpActionResult EditCabinet(Cabinet cabinet)
        {
            try
            {
                string valiate = ValiateCabinetModel(cabinet);
                if (!string.IsNullOrEmpty(valiate))
                {
                    return Failure(valiate);
                }
                if (cabinet.ID == 0)
                    return Failure("未指定保险柜");
                var cab = Cabinet.GetOne(cabinet.ID);
                if (cab == null)
                    return Failure("未找到指定保险柜");
                cab.Address = cabinet.Address;
                cab.AndroidMac = cabinet.AndroidMac;
                cab.Code = cabinet.Code;
                cab.DepartmentID = cabinet.DepartmentID;
                cab.FirstContact = cabinet.FirstContact;
                cab.FirstContactPassword = AESAlgorithm.Encrypto(cabinet.FirstContactPassword);
                cab.FirstContactPhone = cabinet.FirstContactPhone;

                cab.IP = cabinet.IP;
                cab.Name = cabinet.Name;
                cab.NeedConfirm = cabinet.NeedConfirm;
                cab.Remark = cabinet.Remark;
                cab.SecondContact = cabinet.SecondContact;

                cab.SecondContactPassword = AESAlgorithm.Encrypto(cabinet.SecondContactPassword);
                cab.SecondContactPhone = cabinet.SecondContactPhone;

                Cabinet.Update(cab);
                return Success(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("修改失败");
            }
        }

        [HttpPost, Route("api/cabinet/delete")]
        public IHttpActionResult DeleteCabinet(int cabinetID)
        {
            if (cabinetID == 0)
            {
                return Failure("未指定保险柜");
            }
            try
            {
                Cabinet.Delete(cabinetID);
                return Success(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Failure("删除失败");
            }
        }

        [HttpPost, Route("api/cabinet/view")]
        public IHttpActionResult QueryCabinet(CabinetSearchModel search)
        {
            try
            {
                var result = Cabinet.GetCabinets(search);
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