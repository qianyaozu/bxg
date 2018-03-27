using CabinetData.Base;
using System;
using System.Collections.Generic;

namespace CabinetData.Entities
{
    public partial class CabinetA : Cabinet
    {
        public string DepartmentName { get; set; }

    }
    public partial class Cabinet : BaseModel
	{

		/// <summary>
		/// 
		/// </summary>
		public Int32 ID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String Name { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String Address { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Int32 DepartmentID { get; set; }

		/// <summary>
		/// 保险柜号码
		/// </summary>
		public String Code { get; set; }

		/// <summary>
		/// 保险柜Android硬件mac地址
		/// </summary>
		public String AndroidMac { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String IP { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Boolean? IsOnline { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? LastOnlineTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int? Status { get; set; }

		/// <summary>
		/// 是否需要确认开门
		/// </summary>
		public Boolean? NeedConfirm { get; set; }

		/// <summary>
		/// android版本
		/// </summary>
		public String AndroidVersion { get; set; }

		/// <summary>
		/// 负责人
		/// </summary>
		public String FirstContact { get; set; }

		/// <summary>
		/// 负责人电话
		/// </summary>
		public String FirstContactPhone { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String FirstContactPassword { get; set; }

		/// <summary>
		/// 负责人
		/// </summary>
		public String SecondContact { get; set; }

		/// <summary>
		/// 负责人电话
		/// </summary>
		public String SecondContactPhone { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String SecondContactPassword { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String Remark { get; set; }

        public DateTime? CreateTime { get; set; }
         

    }
    public class CabinetLogA: CabinetLog
    {
        public string CabinetCode { get; set; }
        public string DepartmentName { get; set; }
    }

    public partial class CabinetLog : BaseModel
	{

		/// <summary>
		/// 
		/// </summary>
		public Int32 ID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Int32 CabinetID { get; set; }

        public int? DepartmentID { get; set; }
                                           /// <summary>
                                           /// 
                                           /// </summary>
        public String OperatorName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? OperateTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Int32 OperationType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime CreateTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String CabinetIP { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String EventContent { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String DealPerson { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? DealTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String DealResult { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String Remark { get; set; }

	}

	 
 

	public partial class Role_Module : BaseModel
	{

		/// <summary>
		/// 
		/// </summary>
		public Int32 ID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Int32 RoleID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Int32 ModuleID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String ModuleName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Boolean EnableView { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Boolean EnableAdd { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Boolean EnableEdit { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Boolean EnableDelete { get; set; }

	}

    public class DepartmentTree
    {
        public int ID { get; set; }
        public string label { get; set; }

        public string ParentName { get; set; }
        public List<DepartmentTree> children { get; set; }
    }
    public partial class DepartmentA : Department
    {

        /// <summary>
        /// 
        /// </summary>
        public Int32 ID { get; set; }
        public string ParentName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int32? ParentID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Int32? SortID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String Address { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String Remark { get; set; }


        public string CenterIP { get; set; }

    }
    public partial class Department : BaseModel
	{

		/// <summary>
		/// 
		/// </summary>
		public Int32 ID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Int32? ParentID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String Name { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Int32? SortID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String Address { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String Remark { get; set; }


        public string CenterIP { get; set; }

    }
    public class UserInfoA : UserInfo
    {
        public string DepartmentName { get; set; }
    }
    public partial class UserInfo : BaseModel
	{

		/// <summary>
		/// 
		/// </summary>
		public Int32 ID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String Name { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String Password { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Int32 DepartmentID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Int32 RoleID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String LastLoginIP { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? LastLoginTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String RealName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String Phone { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String Email { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? CreateTime { get; set; }

		/// <summary>
		/// 1为启用，0为禁用
		/// </summary>
		public Int32? Status { get; set; }

	}

	public partial class SystemLog : BaseModel
	{

		/// <summary>
		/// 
		/// </summary>
		public Int32 ID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String Action { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String LogContent { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime CreateTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Int32? UserID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String UserName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String RealName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int DepartmentID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int RoleID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public String ClientIP { get; set; }

	}

}
