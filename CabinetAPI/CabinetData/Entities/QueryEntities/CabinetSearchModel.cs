using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CabinetData.Entities.QueryEntities
{
   public class CabinetSearchModel
    {
        /// <summary>
        /// 部门编号
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// 保险柜名称
        /// </summary>
        public string CabinetName { get; set; }

        /// <summary>
        /// 保险柜编号
        /// </summary>
        public string CabinetCode { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}
