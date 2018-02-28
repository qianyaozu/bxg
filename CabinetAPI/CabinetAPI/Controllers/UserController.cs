using CabinetData.Entities.QueryEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CabinetAPI.Controllers
{
    public class UserController: BaseController
    {

        [HttpPost, Route("api/user/login")]
        public IHttpActionResult Login(LoginModel model)
        {
            
            return null;
        }

    }
}