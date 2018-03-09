using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CabinetData.Entities.QueryEntities
{
   public class SystemLogSearchModel
    { 
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }


        public string StartTime { get; set; }
        public string EndTime { get; set; }


        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}
