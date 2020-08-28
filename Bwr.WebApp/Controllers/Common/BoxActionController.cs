using BWR.Application.Dtos.BoxAction;
using BWR.Application.Interfaces.BoxAction;
using BWR.Application.Interfaces.Treasury;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers
{
    public class BoxActionController : Controller
    {
        private readonly IBoxActionAppService _boxActionAppService;
        private readonly ITreasuryAppService _treasuryAppService;
        private string _message;
        private bool _success;

        public BoxActionController(IBoxActionAppService boxActionAppService, ITreasuryAppService treasuryAppService)
        {
            _boxActionAppService = boxActionAppService;
            _treasuryAppService = treasuryAppService;
            _message = "";
            _success = false;
        }
        
        [Authorize]
        public ActionResult Index()
        {
            if (!CheckTreasury())
                return RedirectToAction("NoTreasury", "Home");

            var boxActionInitialDto = _boxActionAppService.InitialInputData();

            return View(boxActionInitialDto);
        }

        [HttpPost]
        public ActionResult PayExpenciveFromMainBox(BoxActionExpensiveDto dto)
        {
            if (_boxActionAppService.PayExpenciveFromMainBox(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult ReciverIncomeToMainBox(BoxActionIncomeDto dto)
        {
            if (_boxActionAppService.ReciverIncomeToMainBox(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult PayForClientFromMainBox(BoxActionClientDto dto)
        {
            if (_boxActionAppService.PayForClientFromMainBox(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult ReciveFromClientToMainBox(BoxActionClientDto dto)
        {
            if (_boxActionAppService.ReciveFromClientToMainBox(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult ReciveFromCompanyToMainBox(BoxActionCompanyDto dto)
        {
            if (_boxActionAppService.ReciveFromCompanyToMainBox(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult PayForCompanyFromMainBox(BoxActionCompanyDto dto)
        {
            if (_boxActionAppService.PayForCompanyFromMainBox(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult FromClientToClient(BoxActionFromClientToClientDto dto)
        {
            if (_boxActionAppService.FromClientToClient(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FromCompanyToClient(BoxActionFromCompanyToClientDto dto)
        {
            if (_boxActionAppService.FromCompanyToClient(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FromClientToCompany(BoxActionFromClientToCompanyDto dto)
        {
            if (_boxActionAppService.FromClientToCompany(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FromCompanyToCompany(BoxActionFromCompanyToCompanyDto dto)
        {
            if (_boxActionAppService.FromCompanyToCompany(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }

        private bool CheckTreasury()
        {
            var currentTreasury = Session["CurrentTreasury"];
            if (currentTreasury != null && currentTreasury.ToString() != "0")
                return true;
            else
            {
                var treasuryDto = _treasuryAppService.GetTreasuryForUser(User.Identity.Name);
                if (treasuryDto != null)
                {
                    Session["CurrentTreasury"] = treasuryDto.Id;
                    return true;
                }
            }

            return false;
        }
    }
}