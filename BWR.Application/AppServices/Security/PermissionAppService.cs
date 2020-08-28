using AutoMapper;
using BWR.Application.Dtos.Permission;
using BWR.Application.Dtos.Role;
using BWR.Application.Interfaces.Security;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Security;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.AppServices.Security
{
    public class PermissionAppService : IPermissionAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;
        private readonly IRoleAppService _roleAppService;

        public PermissionAppService(
            IUnitOfWork<MainContext> unitOfWork, 
            IAppSession appSession,
            IRoleAppService roleAppService)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
            _roleAppService = roleAppService;
        }

        public IList<PermissionDto> GetForSpecificRole(Guid roleId)
        {
            var permissionsDto = new List<PermissionDto>();
            try
            {
                var permissions = _unitOfWork.GenericRepository<Permission>().FindBy(x => x.Role.RoleId == roleId).ToList();
                permissionsDto = Mapper.Map<List<Permission>, List<PermissionDto>>(permissions);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return permissionsDto;
        }

        public IList<PermissionDto> SavePermissions(Guid roleId, IList<string> permissions)
        {
            var permissionsDto = new List<PermissionDto>();
            try
            {
                var role = _unitOfWork.GenericRepository<Role>().GetById(roleId);
                
                CheckForDelete(permissions, role);
                CheckForAdd(permissions, role);

                var newPermissions = _unitOfWork.GenericRepository<Permission>().FindBy(x => x.Role.RoleId == roleId).ToList();
                permissionsDto = Mapper.Map<List<Permission>, List<PermissionDto>>(newPermissions);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return permissionsDto;
        }

        private void CheckForDelete(IList<string> permissions, Role role)
        {
            _unitOfWork.CreateTransaction();

            var rolePermissions = _unitOfWork.GenericRepository<Permission>().FindBy(x => x.Role.RoleId == role.RoleId);
            var permissionsMustDelete = new List<Permission>();

            if (permissions!=null || permissions.Any())
            {
                permissionsMustDelete = rolePermissions.Where(x => !permissions.Contains(x.Name)).ToList();
            }
            else
            {
                permissionsMustDelete = rolePermissions.ToList();
            }
            
            foreach (var permissionMustDelete in permissionsMustDelete)
            {
                _unitOfWork.GenericRepository<Permission>().Delete(permissionMustDelete);
            }

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        private void CheckForAdd(IList<string> permNames, Role role)
        {
            _unitOfWork.CreateTransaction();
            foreach (var permName in permNames)
            {
                var perms = _unitOfWork.GenericRepository<Permission>().FindBy(x => x.Role.RoleId == role.RoleId && x.Name == permName);
                if(perms.Count()==0 || perms == null)
                {
                    var permission = new Permission()
                    {
                        Name = permName,
                        GrantedByUser = _appSession.GetUserName(),
                        GrantedDate = DateTime.Now,
                        Role=role
                    };

                    _unitOfWork.GenericRepository<Permission>().Insert(permission);
                }
            }

            _unitOfWork.Save();
            _unitOfWork.Commit();

        }

        public IList<PermissionDto> Get(Expression<Func<Permission, bool>> predicate)
        {
            List<PermissionDto> PermissionsDto = null;
            try
            {
                var permissions = _unitOfWork.GenericRepository<Permission>().FindBy(predicate).ToList();
                PermissionsDto = Mapper.Map<List<Permission>, List<PermissionDto>>(permissions);
            }
            catch(Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return PermissionsDto;
        }
    }
}
