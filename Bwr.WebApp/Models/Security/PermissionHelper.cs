using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bwr.WebApp.Models.Security
{
    public class PermissionHelper
    {
       public static bool CheckPermission(string permission)
        {
            var permissions = (IList<string>)HttpContext.Current.Session["UserPermissions"];

            if(permissions!=null)
                return permissions.Contains(permission);

            return false;
        }
        
    }
}
