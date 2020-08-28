using System;
using System.Collections.Generic;
using BWR.Application.Dtos.Role;
namespace BWR.Application.Interfaces.Security
{
    public interface IRoleAppService 
    {
        IList<RoleForDropdownDto> GetForDropdown(string name);
        bool CheckIfExist(string name, string id);
        IList<RoleDto> GetAll();
        IList<RoleDto> GetRolesForSpecificUser(Guid userId);
        IList<RoleDto> GetRolesForSpecificUser(string userName);
        RoleDto GetById(Guid id);
        RoleDto Insert(RoleInsertDto dto);
        RoleDto Update(RoleUpdateDto dto);
        RoleUpdateDto GetForEdit(Guid id);
        void Delete(Guid id);
        IList<RoleDto> AssignRolesToUser(IList<RoleDto> rolesDto, Guid userId);
    }
}
