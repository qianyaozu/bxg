using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CabinetData.Entities.QueryEntities
{
   public class CabinetLogSearchModel
    { 

        public int? CabinetID { get; set; }
        /// <summary>
        /// 保险柜名称
        /// </summary>
        public string CabinetName { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        public int? DepartmentID { get; set; }
        /// <summary>
        /// 报警类型
        /// </summary>
        public string OperatorType { get; set; }


        public string StartTime { get; set; }
        public string EndTime { get; set; }


        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}
