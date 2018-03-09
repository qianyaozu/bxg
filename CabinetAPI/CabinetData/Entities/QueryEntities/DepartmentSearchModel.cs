using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CabinetData.Entities.QueryEntities
{
    public class DepartmentSearchModel
    {
        public string DepartmentName { get; set; }

        public int? ParentID { get; set; }


        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}
