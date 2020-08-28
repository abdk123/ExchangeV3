using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Domain.Model.Security
{
    public class Role
    {
        #region Fields
        private ICollection<User> _users;
        private ICollection<Permission> _permission;
        #endregion

        #region Scalar Properties
        
        public Guid RoleId { get; set; }
        public string Name { get; set; }
        #endregion

        #region Navigation Properties
        public ICollection<User> Users
        {
            get { return _users ?? (_users = new List<User>()); }
            set { _users = value; }
        }
        public ICollection<Permission> Permissions
        {
            get { return _permission ?? (_permission = new List<Permission>()); }
            set { _permission = value; }
        }
        #endregion
    }
}
