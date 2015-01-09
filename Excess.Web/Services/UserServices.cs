using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace Excess.Web.Services
{
    public class UserService : IUserServices
    {
        public string userId()
        {
            return HttpContext.Current.User.Identity.GetUserId();
        }
    }
}