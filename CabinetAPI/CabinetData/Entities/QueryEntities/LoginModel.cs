﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CabinetData.Entities.QueryEntities
{
    public class LoginModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public DateTime CreateTime { get; set; }
    }

    
}
