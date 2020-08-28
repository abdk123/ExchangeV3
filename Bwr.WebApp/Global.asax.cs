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
            if (!_firstOne)
            {
                _firstOne = true;
                var unitOfWork = new UnitOfWork<MainContext>();
                var repository = new GenericRepository<Branch>(unitOfWork);

                if (!repository.GetAll().Any())
                {
                    var newBranch = new Branch()
                    {
                        Name = "الفرع الرئيسي",
                        Address = "Address"
                    };

                    repository.Insert(newBranch);
                    repository.Save();
                }


                var branch = repository.GetAll().FirstOrDefault();
                if (branch != null)
                {
                    BranchHelper.Id = branch.Id;
                    BranchHelper.Name = branch.Name;
                    BranchHelper.CountryId = branch.CountryId;
                }
            }
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
            }
        }
    }
}
