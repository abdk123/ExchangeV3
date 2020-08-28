using System.Web;

namespace Bwr.WebApp.Models.Security
{
    public static class UserHelper
    {
        public static string GetUserName()
        {
            if (HttpContext.Current != null && HttpContext.Current.User != null)
            {
                return HttpContext.Current.User.Identity.Name;
            }
            return string.Empty;
        }
    }
}