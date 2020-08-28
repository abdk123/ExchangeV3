using System.Web.Mvc;

namespace Bwr.WebApp.Controllers
{

    public class SystemController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

    }
}