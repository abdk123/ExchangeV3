using BWR.Application.Dtos.Treasury;
using BWR.Application.Interfaces.Treasury;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using DataTables.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers.Treasury
{
    public class TreasuryCashController : Controller
    {
        private readonly ITreasuryCashAppService _treasuryCashAppService;
        public TreasuryCashController(ITreasuryCashAppService treasuryCashAppService)
        {
            _treasuryCashAppService = treasuryCashAppService;
        }

        
        public ActionResult Index(int treasuryId)
        {

            return View();
        }

        // GET: TreasuryCashFlow
        public ActionResult Get(int treasuryId)
        {
            var treasuryCashes = _treasuryCashAppService.GetTreasuryCashes(treasuryId);
            return Json(new { data = treasuryCashes }, JsonRequestBehavior.AllowGet);
        }

        
    }
}