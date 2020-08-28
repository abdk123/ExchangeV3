using Microsoft.Owin;
using BWR.ShareKernel.Permisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers
{
    public class BwrController : Controller
    {
        private readonly IList<string> _options;

        public BwrController()
        {
            this._options = typeof(AppPermision).GetFields(BindingFlags.Public | BindingFlags.Static |
                     BindingFlags.FlattenHierarchy).
                     Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(x => x.GetValue(null).ToString()).ToList();

        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var action = filterContext.RouteData.Values["action"].ToString();
            var controller = filterContext.RouteData.Values["controller"].ToString();

            if (!CheckPagePermission(controller, action) || !CheckActionPermission(controller, action))
            {
                var loginPath = new PathString("/Account/Login");
                string url = loginPath.Value;
                System.Web.HttpContext.Current.Response.Redirect(url, true);
            }

            var permissions = (IList<string>)Session["UserPermissions"];
            if (permissions != null)
                ViewBag.Authorize = permissions.Where(x => x.Contains(controller));
        }

        private bool CheckPagePermission(string controller, string action)
        {
            var permissions = (IList<string>)Session["UserPermissions"];
            string page;
            if (action.ToLower() == "index")
                page = string.Format("Pages.{0}", controller);
            else
                page = string.Format("Pages.{0}.{1}", controller, action);

           
            if (permissions == null || (_options.Contains(page) && !permissions.Contains(page)))
                return false;

            return true;
        }

        private bool CheckActionPermission(string controller, string action)
        {
            var permissions = (IList<string>)Session["UserPermissions"];
            var permPage = string.Format("Action.{0}.{1}", controller, action);
            if (permissions == null || (_options.Contains(permPage) && !permissions.Contains(permPage)))
                return false;

            return true;
        }
    }
}