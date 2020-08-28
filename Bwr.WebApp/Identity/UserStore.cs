using BWR.Domain.Model.Security;
using BWR.Domain.Repositories;
using BWR.Infrastructure.Common;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Repositories;
using BWR.ShareKernel.Interfaces;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Claim = System.Security.Claims.Claim;

namespace Bwr.WebApp.Identity
{
    public class UserStore : IUserLoginStore<IdentityUser, Guid>, IUserClaimStore<IdentityUser, Guid>, IUserRoleStore<IdentityUser, Guid>, IUserPasswordStore<IdentityUser, Guid>, IUserSecurityStampStore<IdentityUser, Guid>, IUserStore<IdentityUser, Guid>, IDisposable
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IExternalLoginRepository _externalLoginRepository;
        private readonly IRoleRepository _roleRepository;

        public UserStore(IUnitOfWork<MainContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userRepository = new UserRepository(_unitOfWork);
            _roleRepository = new RoleRepository(_unitOfWork);
            _externalLoginRepository = new ExternalLoginRepository(_unitOfWork); ;
        }

        #region IUserStore<IdentityUser, int> Members
        public Task CreateAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var u = getUser(user);

            _userRepository.Insert(u);
            _unitOfWork.Save();
            return Task.Delay(50);
        }

        public Task DeleteAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var u = getUser(user);

            _userRepository.Delete(u);
            _unitOfWork.Save();
            return Task.Delay(50);
        }

        public Task<IdentityUser> GetByIdAsync(Guid userId)
        {
            var user = _userRepository.GetById(userId);
            return Task.FromResult<IdentityUser>(GetIdentityUser(user));
        }

        public Task<IdentityUser> FindByNameAsync(string userName)
        {
            var user = _userRepository.FindBy(x => x.UserName == userName).FirstOrDefault();
            return Task.FromResult<IdentityUser>(GetIdentityUser(user));
        }

        public Task<IdentityUser> FindByIdAsync(Guid userId)
        {
            var user = _userRepository.GetById(userId);
            return Task.FromResult<IdentityUser>(GetIdentityUser(user));
        }

        public Task UpdateAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentException("user");

            var u = _userRepository.GetById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            PopulateUser(u, user);

            _userRepository.Update(u);
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

        #region IUserClaimStore<IdentityUser, Guid> Members
        public Task AddClaimAsync(IdentityUser user, Claim claim)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (claim == null)
                throw new ArgumentNullException("claim");

            var u = _userRepository.GetById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            var c = new BWR.Domain.Model.Security.Claim
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                User = u
            };
            u.Claims.Add(c);

            _userRepository.Update(u);
            _unitOfWork.Save();
            return Task.Delay(50);
        }

        public Task<IList<Claim>> GetClaimsAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var u = _userRepository.GetById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            return Task.FromResult<IList<Claim>>(u.Claims.Select(x => new Claim(x.ClaimType, x.ClaimValue)).ToList());
        }

        public Task RemoveClaimAsync(IdentityUser user, Claim claim)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (claim == null)
                throw new ArgumentNullException("claim");

            var u = _userRepository.GetById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            var c = u.Claims.FirstOrDefault(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            u.Claims.Remove(c);

            _userRepository.Update(u);
            _unitOfWork.Save();
            return Task.Delay(50);
        }
        #endregion

        #region IUserLoginStore<IdentityUser, int> Members
        public Task AddLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (login == null)
                throw new ArgumentNullException("login");

            var u = _userRepository.GetById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            var l = new ExternalLogin
            {
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                User = u
            };
            u.Logins.Add(l);

            _userRepository.Update(u);
            _unitOfWork.Save();
            return Task.Delay(50);
        }

        public Task<IdentityUser> FindAsync(UserLoginInfo login)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            var identityUser = default(IdentityUser);

            var l = _externalLoginRepository.GetByProviderAndKey(login.LoginProvider, login.ProviderKey);
            if (l != null)
                identityUser = GetIdentityUser(l.User);

            return Task.FromResult<IdentityUser>(identityUser);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var u = _userRepository.GetById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            return Task.FromResult<IList<UserLoginInfo>>(u.Logins.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey)).ToList());
        }

        public Task RemoveLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (login == null)
                throw new ArgumentNullException("login");

            var u = _userRepository.GetById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            var l = u.Logins.FirstOrDefault(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
            u.Logins.Remove(l);

            _userRepository.Update(u);
            _unitOfWork.Save();
            return Task.Delay(50);
        }
        #endregion

        #region IUserRoleStore<IdentityUser, int> Members
        public Task AddToRoleAsync(IdentityUser user, string roleName)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Argument cannot be null, empty, or whitespace: roleName.");

            var u = _userRepository.GetById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");
            var r = _roleRepository.FindBy(x => x.Name == roleName).FirstOrDefault();
            if (r == null)
                throw new ArgumentException("roleName does not correspond to a Role entity.", "roleName");

            u.Roles.Add(r);
            _userRepository.Update(u);

            _unitOfWork.Save();
            return Task.Delay(50);
        }

        public Task<IList<string>> GetRolesAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var u = _userRepository.GetById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            return Task.FromResult<IList<string>>(u.Roles.Select(x => x.Name).ToList());
        }

        public Task<bool> IsInRoleAsync(IdentityUser user, string roleName)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Argument cannot be null, empty, or whitespace: role.");

            var u = _userRepository.GetById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            return Task.FromResult<bool>(u.Roles.Any(x => x.Name == roleName));
        }

        public Task RemoveFromRoleAsync(IdentityUser user, string roleName)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Argument cannot be null, empty, or whitespace: role.");

            var u = _userRepository.GetById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            var r = u.Roles.FirstOrDefault(x => x.Name == roleName);
            u.Roles.Remove(r);

            _userRepository.Update(u);
            _unitOfWork.Save();
            return Task.Delay(50);
        }
        #endregion

        #region IUserPasswordStore<IdentityUser, int> Members
        public Task<string> GetPasswordHashAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult<string>(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult<bool>(!string.IsNullOrWhiteSpace(user.PasswordHash));
        }

        public Task SetPasswordHashAsync(IdentityUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }
        #endregion

        #region IUserSecurityStampStore<IdentityUser, int> Members
        public Task<string> GetSecurityStampAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult<string>(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(IdentityUser user, string stamp)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }
        #endregion

        #region Private Methods
        private User getUser(IdentityUser identityUser)
        {
            if (identityUser == null)
                return null;

            var user = new User();
            PopulateUser(user, identityUser);

            return user;
        }

        private void PopulateUser(User user, IdentityUser identityUser)
        {
            user.UserId = identityUser.Id;
            user.UserName = identityUser.UserName;
            user.PasswordHash = identityUser.PasswordHash;
            user.SecurityStamp = identityUser.SecurityStamp;
            user.Email = identityUser.Email;
            user.FullName = identityUser.FullName;
        }

        private IdentityUser GetIdentityUser(User user)
        {
            if (user == null)
                return null;

            var identityUser = new IdentityUser();
            PopulateIdentityUser(identityUser, user);

            return identityUser;
        }

        private void PopulateIdentityUser(IdentityUser identityUser, User user)
        {
            identityUser.Id = user.UserId;
            identityUser.UserName = user.UserName;
            identityUser.PasswordHash = user.PasswordHash;
            identityUser.SecurityStamp = user.SecurityStamp;
            identityUser.Email = user.Email;
            identityUser.FullName = user.FullName;
        }

        

        #endregion
    }
}