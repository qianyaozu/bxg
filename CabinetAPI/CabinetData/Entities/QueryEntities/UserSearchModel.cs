using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CabinetData.Entities.QueryEntities
{
   public class UserSearchModel
    {
        /// <summary>
        /// 部门编号
        /// </summary>
        public int? DepartmentID { get; set; }
        /// <summary>
        /// 角色编号
        /// </summary>
        public int? RoleID { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}
