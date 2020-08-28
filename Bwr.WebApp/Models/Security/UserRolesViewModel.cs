using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bwr.WebApp.Models.Security
{
    public class UserRolesViewModel
    {
        public Guid RoleId { get; set; }
        public string Name { get; set; }
        public bool IsExist { get; set; }
    }
}