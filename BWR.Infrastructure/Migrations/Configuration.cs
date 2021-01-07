namespace BWR.Infrastructure.Migrations
{
    using BWR.Domain.Model.Branches;
    using BWR.Domain.Model.Security;
    using BWR.Domain.Model.Settings;
    using BWR.Infrastructure.Context;
    using BWR.Domain.Model.Treasures;
    using BWR.ShareKernel.Interfaces;
    using BWR.ShareKernel.Permisions;
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Reflection;

    internal sealed class Configuration : DbMigrationsConfiguration<BWR.Infrastructure.Context.MainContext>
    {

        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            //AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(MainContext context)
        {
            #region admin user
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
                    //admin
                    PasswordHash = "ANF2VrSJI/ZRaK99ymBl/xHO+jkzMVQOogsWHHCpemIEzQBL5AHqcGnlhDysgiDwtg==",
                    SecurityStamp = "718b64fc-3be0-4467-af63-09ca67e911aa",
                    Email = "admin@bwire.com"
                };
                admin.Roles.Add(adminRole);

                context.Users.Add(admin); 
                context.SaveChanges();
            }
            #endregion
            #region country seeder
            if (!context.Countrys.Any())
            {
                var syria = new Country()
                {
                    Name = "سوريا",
                    IsEnabled = true,
                };
                var iraq = new Country()
                {
                    Name = "العراق",
                    IsEnabled = true,
                };
                context.Countrys.AddRange(new[] { syria, iraq });
                context.SaveChanges();
            }
            #endregion
            #region mainBranch
            if (!context.Branchs.Any())
            {
                var mainBranch = new Branch()
                {
                    Name = "الفرع الرئيسي ",
                    Address = "",
                };
                context.Branchs.Add(mainBranch);
                context.SaveChanges();
            }
            if (!context.Treasurys.Any())
            {

            }
            #endregion
            #region coin
            if (!context.Coins.Any())
            {
                var dollar = new Coin()
                {
                    Name = "دولار",
                    IsEnabled = true,
                };
                var irDinar = new Coin()
                {

                    Name = "دينار عراقي",
                    IsEnabled = true
                };
                context.Coins.AddRange(new[] { dollar, irDinar });
                context.SaveChanges();
                var total = 1000000;
                var brancheId = context.Branchs.First().Id;
                var sBranchCash = new BranchCash()
                {
                    CoinId = dollar.Id,
                    BranchId = brancheId,
                    InitialBalance = total,
                };
                var iBranchCash = new BranchCash()
                {
                    BranchId = brancheId,
                    CoinId = irDinar.Id,
                    InitialBalance = total
                };
                context.BranchCashs.AddRange(new[] { iBranchCash, sBranchCash });
                context.SaveChanges();
                var trusery = new Treasury()
                {
                    Name = "الصندوق الرئيسي",
                    IsMainTreasury = true,
                    IsAvilable = false,
                    BranchId = brancheId,
                    IsEnabled = true,
                };
                context.Treasurys.Add(trusery);
                var itruseryCash = new TreasuryCash()
                {
                    CoinId = dollar.Id,
                    Total = total,
                    IsEnabled = true,
                    IsDeleted = false,
                };
                var struseryCash = new TreasuryCash()
                {
                    CoinId = irDinar.Id,
                    Total = total,
                    IsEnabled = true,
                    IsDeleted = false,
                };
                context.TreasuryCashs.Add(itruseryCash);
                context.TreasuryCashs.Add(struseryCash);
                var userTreusery = new UserTreasuery()
                {
                    TreasuryId = trusery.Id,
                    UserId = context.Users.First().UserId,
                    IsDeleted = false,
                    CreatedBy = "النظام",
                    Created = DateTime.Now,
                };
                context.UserTreasueries.Add(userTreusery);
            }
            #endregion
        }
    }
}
