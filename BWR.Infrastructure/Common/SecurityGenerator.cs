using BWR.Domain.Model.Security;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Permisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Infrastructure.Common
{
    public class SecurityGenerator
    {
        public static void Start()
        {
            var context = new MainContext();
            var adminRole = context.Roles.FirstOrDefault(x => x.Name.ToLower() == "admin");
            if (adminRole == null)
            {
                adminRole = new Role()
                {
                    Name = "admin",
                    RoleId = Guid.NewGuid()
                };

                context.SaveChanges();
            }
            else
            {
                var permissions = context.Permissions.Where(x => x.Role.Name.ToLower() == "admin");
                context.Permissions.RemoveRange(permissions);
                context.SaveChanges();
            }
            var options = typeof(AppPermision).GetFields(BindingFlags.Public | BindingFlags.Static |
                     BindingFlags.FlattenHierarchy).
                     Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(x => x.GetValue(null).ToString()).ToList();

            foreach (var option in options)
            {
                var permission = new Permission()
                {
                    Name = option,
                    GrantedByUser = "",
                    GrantedDate = DateTime.Now,
                    Role = adminRole
                };

                context.Permissions.Add(permission);
            }
            context.SaveChanges();

            var adminUserExist = context.Users.Any(x => x.UserName.ToLower() == "admin");
            if (!adminUserExist)
            {
                var admin = new User()
                {
                    UserId = Guid.NewGuid(),
                    FullName = "Admin",
                    UserName = "admin",
                    PasswordHash = "ANF2VrSJI/ZRaK99ymBl/xHO+jkzMVQOogsWHHCpemIEzQBL5AHqcGnlhDysgiDwtg==",
                    SecurityStamp = "718b64fc-3be0-4467-af63-09ca67e911aa",
                    Email = "admin@bwire.com"
                };
                admin.Roles.Add(adminRole);

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
