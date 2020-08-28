using System;
using BWR.Application.Common;
using BWR.Application.Dtos.Role;

namespace BWR.Application.Dtos.Permission
{
    public class PermissionUpdateDto : EntityDto
    {
         public string Name { get; set; }
         public RoleDto Role { get; set; }
         public string GrantedByUser { get; set; }
         public DateTime? GrantedDate { get; set; }
    }
}
