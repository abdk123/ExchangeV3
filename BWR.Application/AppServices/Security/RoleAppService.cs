using AutoMapper;
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

namespace BWR.Application.AppServices.Security
{
    public class RoleAppService : IRoleAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public RoleAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public IList<RoleDto> GetAll()
        {
            var rolesDtos = new List<RoleDto>();
            try
            {
                var roles = _unitOfWork.GenericRepository<Role>().GetAll().ToList();
                rolesDtos = Mapper.Map<List<Role>, List<RoleDto>>(roles);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return rolesDtos;
        }

        public IList<RoleDto> GetRolesForSpecificUser(Guid userId)
        {
            var rolesDtos = new List<RoleDto>();
            try
            {
                var roles = _unitOfWork.GenericRepository<Role>().FindBy(x => x.Users.Any(y => y.UserId == userId)).ToList();
                rolesDtos = Mapper.Map<List<Role>, List<RoleDto>>(roles);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return rolesDtos;
        }

        public IList<RoleDto> GetRolesForSpecificUser(string userName)
        {
            var rolesDtos = new List<RoleDto>();
            try
            {
                var roles = _unitOfWork.GenericRepository<Role>().FindBy(x => x.Users.Any(y => y.UserName == userName)).ToList();
                rolesDtos = Mapper.Map<List<Role>, List<RoleDto>>(roles);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return rolesDtos;
        }

        public RoleDto GetById(Guid id)
        {
            RoleDto roleDto = null;
            try
            {
                var role = _unitOfWork.GenericRepository<Role>().GetById(id);
                if (role != null)
                {
                    roleDto = Mapper.Map<Role, RoleDto>(role);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return roleDto;
        }

        public IList<RoleForDropdownDto> GetForDropdown(string name)
        {
            var rolesDtos = new List<RoleForDropdownDto>();
            try
            {
                var roles = _unitOfWork.GenericRepository<Role>().FindBy(x => x.Name.StartsWith(name)).ToList();
                Mapper.Map<List<Role>, List<RoleForDropdownDto>>(roles, rolesDtos);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return rolesDtos;
        }

        public RoleUpdateDto GetForEdit(Guid id)
        {
            RoleUpdateDto roleDto = null;
            try
            {
                var role = _unitOfWork.GenericRepository<Role>().GetById(id);
                if (role != null)
                {
                    roleDto = Mapper.Map<Role, RoleUpdateDto>(role);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return roleDto;
        }

        public RoleDto Insert(RoleInsertDto dto)
        {
            RoleDto roleDto = null;
            try
            {
                var role = Mapper.Map<RoleInsertDto, Role>(dto);
                role.RoleId = Guid.NewGuid();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Role>().Insert(role);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                roleDto = Mapper.Map<Role, RoleDto>(role);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return roleDto;
        }

        public RoleDto Update(RoleUpdateDto dto)
        {
            RoleDto roleDto = null;
            try
            {
                var role = _unitOfWork.GenericRepository<Role>().GetById(dto.RoleId);
                Mapper.Map<RoleUpdateDto, Role>(dto, role);
                //role.ModifiedBy = _appSession.GetRoleName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Role>().Update(role);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                roleDto = Mapper.Map<Role, RoleDto>(role);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return roleDto;
        }

        public void Delete(Guid id)
        {
            try
            {
                var role = _unitOfWork.GenericRepository<Role>().GetById(id);
                if (role != null)
                {
                    _unitOfWork.GenericRepository<Role>().Delete(role);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }

        public bool CheckIfExist(string roleName, string id)
        {
            try
            {
                var role = _unitOfWork.GenericRepository<Role>()
                    .FindBy(x => x.Name.Trim().Equals(roleName.Trim()) && x.RoleId.ToString() != id)
                    .FirstOrDefault();
                if (role != null)
                    return true;
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return false;
        }

        public IList<RoleDto> AssignRolesToUser(IList<RoleDto> rolesDto, Guid userId)
        {
            IList<RoleDto> newRolesDto = null;
            try
            {
                var user = _unitOfWork.GenericRepository<User>().GetById(userId);

                CheckForDelete(rolesDto, user);
                CheckForAdd(rolesDto, user);

                var roles = _unitOfWork.GenericRepository<Role>().FindBy(x => x.Users.Any(y=>y.UserId==userId)).ToList();
                newRolesDto = Mapper.Map<List<Role>, List<RoleDto>>(roles);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return newRolesDto;
        }

        private void CheckForDelete(IList<RoleDto> rolesDto, User user)
        {
            var rolesMustDelete = new List<Role>();
            _unitOfWork.CreateTransaction();
            if(rolesDto == null || !rolesDto.Any())
            {
                rolesMustDelete = user.Roles.ToList();
            }
            else
            {
                rolesMustDelete = user.Roles.Where(x => !rolesDto.Select(y => y.Name).Contains(x.Name)).ToList();
            }
            
            foreach (var roleMustDelete in rolesMustDelete)
            {
                var role = _unitOfWork.GenericRepository<Role>().GetById(roleMustDelete.RoleId);
                user.Roles.Remove(role);
            }
            _unitOfWork.GenericRepository<User>().Update(user);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        private void CheckForAdd(IList<RoleDto> rolesDto, User user)
        {
            if (rolesDto != null)
            {
                _unitOfWork.CreateTransaction();
                foreach (var roleDto in rolesDto)
                {
                    var perms = _unitOfWork.GenericRepository<Role>().FindBy(x => x.Users.Any(y => y.UserId == user.UserId) && x.Name == roleDto.Name);
                    if (perms.Count() == 0 || perms == null)
                    {
                        var role = _unitOfWork.GenericRepository<Role>().GetById(roleDto.RoleId);
                        user.Roles.Add(role);
                    }
                }
                _unitOfWork.GenericRepository<User>().Update(user);
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }
            
        }

        
    }
}
