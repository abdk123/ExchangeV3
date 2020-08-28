using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers
{
    public class SecurityController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}