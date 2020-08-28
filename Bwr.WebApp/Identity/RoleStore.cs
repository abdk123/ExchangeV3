using BWR.Domain.Model.Security;
using BWR.Domain.Repositories;
using BWR.Infrastructure.Common;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Repositories;
using BWR.ShareKernel.Interfaces;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bwr.WebApp.Identity
{
    public class RoleStore : IRoleStore<IdentityRole, Guid>, IQueryableRoleStore<IdentityRole, Guid>, IDisposable
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IRoleRepository _roleRepository;

        public RoleStore()
        {
            _unitOfWork = new UnitOfWork<MainContext>();
            _roleRepository = new RoleRepository(_unitOfWork);
        }

        #region IRoleStore<IdentityRole, int> Members
        public Task CreateAsync(IdentityRole role)
        {
            if (role == null)
                throw new ArgumentNullException("role");

            var r = getRole(role);

            _roleRepository.Insert(r);
            _unitOfWork.Save();
            return Task.Delay(50);
        }

        public Task DeleteAsync(IdentityRole role)
        {
            if (role == null)
                throw new ArgumentNullException("role");

            var r = getRole(role);

            _roleRepository.Delete(r);
            _unitOfWork.Save();
            return Task.Delay(50);
        }

        public Task<IdentityRole> FindByIdAsync(Guid roleId)
        {
            var role = _roleRepository.GetById(roleId);
            return Task.FromResult<IdentityRole>(getIdentityRole(role));
        }

        public Task<IdentityRole> FindByNameAsync(string roleName)
        {
            var role = _roleRepository.GetAll().FirstOrDefault(x => x.Name == roleName);
            return Task.FromResult<IdentityRole>(getIdentityRole(role));
        }

        public Task UpdateAsync(IdentityRole role)
        {
            if (role == null)
                throw new ArgumentNullException("role");
            var r = getRole(role);
            _roleRepository.Update(r);
            _unitOfWork.Save();
            return Task.Delay(50);
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            // Dispose does nothing since we want Unity to manage the lifecycle of our Unit of Work
        }
        #endregion

        #region IQueryableRoleStore<IdentityRole, int> Members
        public IQueryable<IdentityRole> Roles
        {
            get
            {
                return _roleRepository
                    .GetAll()
                    .Select(x => getIdentityRole(x))
                    .AsQueryable();
            }
        }
        #endregion

        #region Private Methods
        private Role getRole(IdentityRole identityRole)
        {
            if (identityRole == null)
                return null;
            return new Role
            {
                RoleId = identityRole.Id,
                Name = identityRole.Name
            };
        }

        private IdentityRole getIdentityRole(Role role)
        {
            if (role == null)
                return null;
            return new IdentityRole
            {
                Id = role.RoleId,
                Name = role.Name
            };
        }

        
        #endregion
    }
}