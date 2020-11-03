using BWR.Domain.Model.Security;
using BWR.Infrastructure.Common;
using BWR.Infrastructure.Context;
using Castle.Windsor;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Bwr.WebApp.Windsor;
using BWR.Application.Dtos.Branch;
using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Treasures;
using BWR.Domain.Model.Settings;

namespace Bwr.WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static IWindsorContainer container;
        private static bool _firstOne;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MapperConfig.Map();
            BootstrapContainer();

        }

        private static void BootstrapContainer()
        {
            container = new WindsorContainer()
                .Install(FromAssembly.This());
            var controllerFactory = new WindsorControllerFactory(container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //if (!_firstOne)
            //{
            //    _firstOne = true;
            //    var unitOfWork = new UnitOfWork<MainContext>();
            //    var branchRepository = new GenericRepository<Branch>(unitOfWork);
            //    var treasuryRepository = new GenericRepository<Treasury>(unitOfWork);
            //    var treasuryCashRepository = new GenericRepository<TreasuryCash>(unitOfWork);
            //    var coinRepository = new GenericRepository<Coin>(unitOfWork);

            //    if (!branchRepository.GetAll().Any())
            //    {
            //        var newBranch = new Branch()
            //        {
            //            Name = "الفرع الرئيسي",
            //            Address = "Address"
            //        };

            //        branchRepository.Insert(newBranch);
            //        branchRepository.Save();
            //    }


            //    var branch = branchRepository.GetAll().FirstOrDefault();
            //    if (branch != null)
            //    {
            //        BranchHelper.Id = branch.Id;
            //        BranchHelper.Name = branch.Name;
            //        BranchHelper.CountryId = branch.CountryId;
            //    }

            //    if (!treasuryRepository.FindBy(x=>x.IsMainTreasury).Any())
            //    {
            //        var treasury = new Treasury()
            //        {
            //            IsEnabled = true,
            //            IsMainTreasury = true,
            //            BranchId = branch.Id,
            //            Name = "الصندوق الرئيسي"
            //        };

                //    treasuryRepository.Insert(treasury);
                //    treasuryRepository.Save();

                //    var coins = coinRepository.GetAll();

                //    foreach(var coin in coins)
                //    {
                //        var treasuryCash = new TreasuryCash
                //        {
                //            CoinId = coin.Id,
                //            Total = 0,
                //            Treasury = treasury,

                //        };

                //        treasuryCashRepository.Insert(treasuryCash);
                //    }

                //    treasuryCashRepository.Save();
                //}
            //}
        }

        protected void Session_Start(object sender, EventArgs e)
        {

            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var unitOfWork = new UnitOfWork<MainContext>();
                var userName = HttpContext.Current.User.Identity.Name;
                var permissionRepository = new GenericRepository<Permission>(unitOfWork);
                var roleRepository = new GenericRepository<Role>(unitOfWork);

                var roles = roleRepository.FindBy(x => x.Users.Any(y => y.UserName == userName)).Select(x => x.Name).ToList();
                if (roles.Any())
                {
                    Session["UserPermissions"] = permissionRepository.FindBy(x => roles.Contains(x.Role.Name)).Select(x => x.Name).ToList();
                }
                else
                {
                    Session["UserPermissions"] = new List<string>();
                }

                var treasuryRepository= new GenericRepository<Treasury>(unitOfWork);
                var mainTreasury = treasuryRepository.FindBy(x => x.IsMainTreasury).FirstOrDefault();
                if (mainTreasury != null)
                {
                    Session["MainTreasury"] = mainTreasury.Id;
                }
            }
        }
    }
}
