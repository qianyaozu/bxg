using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CabinetData.Entities.QueryEntities
{
    public class Page<T>
    {
        /// <summary>
        /// 当前页。
        /// </summary>
        public long CurrentPage { get; set; }

        /// <summary>
        /// 总页数。
        /// </summary>
        public long TotalPages { get; set; }

        /// <summary>
        /// 总记录数。
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        /// 每页记录数。
        /// </summary>
        public long ItemsPerPage { get; set; }

        /// <summary>
        /// 当前页记录。
        /// </summary>
        public List<T> Items { get; set; }
    }
}
