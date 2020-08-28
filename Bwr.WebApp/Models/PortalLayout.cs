using BWR.Infrastructure.Common;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Repositories;
using Bwr.WebApp.Models.PageModels.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Bwr.WebApp.Models
{
    public class PortalLayout
    {
        
        public UserInfo GetUserInfo()
        {
            var userInfo = new UserInfo();
            if (HttpContext.Current.User.Identity!=null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                string userName = HttpContext.Current.User.Identity.Name;
                var unitOfWork = new UnitOfWork<MainContext>();

                var userRepository = new UserRepository(unitOfWork);
                var user = userRepository.FindBy(x => x.UserName == userName).FirstOrDefault();

                var rootUrl = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                if (user != null)
                {
                    userInfo = new UserInfo()
                    {
                        FullName = user.FullName,
                        UserName = user.UserName,
                        //ImageUrl = !string.IsNullOrEmpty(user.ImageUrl) ? rootUrl+user.ImageUrl : rootUrl+"/Images/user.png"
                    };
                }
            }
            
            return userInfo;
        }

        //public string GetNavigation()
        //{
        //    var strBuilder = new StringBuilder();
        //    strBuilder.Append("<ul class='nav side-menu'>");

        //    var permissions = (IList<string>)HttpContext.Current.Session["UserPermissions"];
        //    if(permissions!=null && permissions.Any())
        //    {
        //        var navigation = new Navigation();
        //        var navigationTabs = navigation.GetNavigationTabs();

        //        foreach (var navigationTab in navigationTabs)
        //        {
        //            if(CheckNavigationTabPermission(navigationTab, permissions))
        //            {
        //                strBuilder.Append("<li>");
        //                strBuilder.Append("<a><i class='" + navigationTab.CssClass + "'></i> " + BwrLocalization.ResourceValue(navigationTab.Name) + " <span class='fa fa-chevron-down'></span></a>");
        //                strBuilder.Append("<ul class='nav child_menu'>");
        //                foreach (var navigationItem in navigationTab.NavigationItems)
        //                {
        //                    if (string.IsNullOrEmpty(navigationItem.Controller))
        //                        navigationItem.Controller = navigationItem.Name;

        //                    if (string.IsNullOrEmpty(navigationItem.Action))
        //                        navigationItem.Action = "Index";


        //                    string page;
        //                    if (navigationItem.Action.ToLower() == "index")
        //                        page = string.Format("Page.{0}", navigationItem.Controller);
        //                    else
        //                        page = string.Format("Page.{0}.{1}", navigationItem.Controller, navigationItem.Action);

        //                    if (permissions.Contains(page))
        //                    {
        //                        var requestContext = HttpContext.Current.Request.RequestContext;
        //                        var urlHelper = new System.Web.Mvc.UrlHelper(requestContext).Action(navigationItem.Action, navigationItem.Controller);

        //                        strBuilder.Append("<li><a href='" + urlHelper + "'>" + BwrLocalization.ResourceValue(navigationItem.Name) + "</a></li>");
        //                    }
        //                }

        //                strBuilder.Append("</ul>");
        //                strBuilder.Append("</li>");
        //            }
        //        }
        //    }
        //    strBuilder.Append("</ul>");

        //    return strBuilder.ToString();
        //}

        //private bool CheckNavigationTabPermission(NavigationTab navigationTab, IList<string> permissions)
        //{
        //    foreach (var navigationItem in navigationTab.NavigationItems)
        //    {
        //        if (string.IsNullOrEmpty(navigationItem.Controller))
        //            navigationItem.Controller = navigationItem.Name;

        //        if (string.IsNullOrEmpty(navigationItem.Action))
        //            navigationItem.Action = "Index";


        //        string page;
        //        if (navigationItem.Action.ToLower() == "index")
        //            page = string.Format("Page.{0}", navigationItem.Controller);
        //        else
        //            page = string.Format("Page.{0}.{1}", navigationItem.Controller, navigationItem.Action);

        //        if (permissions.Contains(page))
        //            return true;
        //    }

        //    return false;
        //}
    }
}