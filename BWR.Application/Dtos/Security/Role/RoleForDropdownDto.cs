using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BWR.Domain.Model.Security;
namespace BWR.Application.Dtos.Role
{
    public class RoleForDropdownDto 
    {
         public Guid RoleId { get; set; }
         public string Name { get; set; }
         
    }
}
