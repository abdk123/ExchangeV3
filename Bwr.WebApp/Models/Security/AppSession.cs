using BWR.Application.Interfaces.Shared;
using System.Web;

namespace Bwr.WebApp.Models.Security
{
    public class AppSession: IAppSession
    {
        public string GetUserName()
        {
            if (HttpContext.Current != null && HttpContext.Current.User != null)
            {
                return HttpContext.Current.User.Identity.Name;
            }
            return string.Empty;
        }

        public int GetCurrentTreasuryId()
        {
            int treasuryId = 0;
            var currentTreasury = HttpContext.Current.Session["CurrentTreasury"];
            if (currentTreasury == null)
                return 0;

            int.TryParse(currentTreasury.ToString(), out treasuryId);
            return treasuryId;
        }
        public int GetMainTreasury()
        {
            int treasuryId = 0;
            var currentTreasury = HttpContext.Current.Session["MainTreasury"];
            if (currentTreasury == null)
                return 0;

            int.TryParse(currentTreasury.ToString(), out treasuryId);
            return treasuryId;
        }
    }
}