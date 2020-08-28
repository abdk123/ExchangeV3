using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bwr.WebApp.Models.Security
{
    public class PermissionViewModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Checked { get; set; }
        public string RoleId { get; set; }
    }
}