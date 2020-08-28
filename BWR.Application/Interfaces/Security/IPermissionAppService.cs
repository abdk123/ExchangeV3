using BWR.Application.Dtos.Permission;
using BWR.Domain.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BWR.Application.Interfaces.Security
{
    public interface IPermissionAppService
    {
        IList<PermissionDto> GetForSpecificRole(Guid roleId);
        IList<PermissionDto> SavePermissions(Guid roleId, IList<string> permissions);
        IList<PermissionDto> Get(Expression<Func<Permission, bool>> predicate);

    }
}
