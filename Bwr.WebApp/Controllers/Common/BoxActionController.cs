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
    [Authorize]
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

        [Authorize]
        public ActionResult BoxActionDetails(int moneyActionId)
        {

            //if (PermissionHelper.CheckPermission(AppPermision.Action_BoxAction_EditBoxAction))
            return RedirectToAction("EditBoxAction", "BoxAction", new { moneyActionId = moneyActionId });
        }

        public ActionResult EditBoxAction(int moneyActionId)
        {
            if (!CheckTreasury())
                return RedirectToAction("NoTreasury", "Home");

            var boxActionInitialDto = _boxActionAppService.InitialInputData();
            ViewBag.MoneyActionId = moneyActionId;

            return View(boxActionInitialDto);
        }

        public ActionResult GetBoxActionForEdit(int moneyActionId)
        {
            var dto = _boxActionAppService.GetForEdit(moneyActionId);

            return Json(dto);
        }

        #region Insert

        [HttpPost]
        public ActionResult PayExpenciveFromMainBox(BoxActionExpensiveDto dto)
        {
            if (_boxActionAppService.ExpenseFromTreasury(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult ReciverIncomeToMainBox(BoxActionIncomeDto dto)
        {
            if (_boxActionAppService.ReceiveToTreasury(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult PayForClientFromMainBox(BoxActionClientDto dto)
        {
            if (_boxActionAppService.ExpenseFromTreasuryToClient(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult ReciveFromClientToMainBox(BoxActionClientDto dto)
        {
            if (_boxActionAppService.ReceiveFromClientToTreasury(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult ReciveFromCompanyToMainBox(BoxActionCompanyDto dto)
        {
            if (_boxActionAppService.ReceiveFromCompanyToTreasury(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult PayForCompanyFromMainBox(BoxActionCompanyDto dto)
        {
            if (_boxActionAppService.ExpenseFromTreasuryToCompany(dto))
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
        [HttpPost]
        public ActionResult FromClientToPublicExpenes(BoxActionFromClientToPublicExpenesDto dto)
        {
            if (_boxActionAppService.ExpenseFromClientToPublic(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult FromClientToPublicIncome(BoxActionFromClientToPublicIncomeDto dto)
        {
            if (_boxActionAppService.ReceiveFromPublicToClient(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult FromCompanyToPublicExpenes(BoxActionFromCompanyToPublicExpenesDto dto)
        {
            if (_boxActionAppService.ExpenseFromCompanyToPublic(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult FromCompanyToPublicIncome(BoxActionFromCompanyToPublicIncomeDto dto)
        {
            if (_boxActionAppService.ReceiveFromPublicToCompany(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Update

        [HttpPost]
        public ActionResult EditPayExpenciveFromMainBox(BoxActionExpensiveUpdateDto dto)
        {
            if (_boxActionAppService.EditExpenseFromTreasury(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult EditReciverIncomeToMainBox(BoxActionIncomeUpdateDto dto)
        {
            if (_boxActionAppService.EditReceiveToTreasury(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult EditPayForClientFromMainBox(BoxActionClientUpdateDto dto)
        {
            if (_boxActionAppService.EditExpenseFromTreasuryToClient(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult EditReciveFromClientToMainBox(BoxActionClientUpdateDto dto)
        {
            if (_boxActionAppService.EditReceiveFromClientToTreasury(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult EditReciveFromCompanyToMainBox(BoxActionCompanyUpdateDto dto)
        {
            if (_boxActionAppService.EditReceiveFromCompanyToTreasury(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult EditPayForCompanyFromMainBox(BoxActionCompanyUpdateDto dto)
        {
            if (_boxActionAppService.EditExpenseFromTreasuryToCompany(dto))
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult EditFromClientToClient(BoxActionFromClientToClientUpdateDto dto)
        {
            if (_boxActionAppService.EditFromClientToClient(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditFromCompanyToClient(BoxActionFromCompanyToClientUpdateDto dto)
        {
            if (_boxActionAppService.EditFromCompanyToClient(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditFromClientToCompany(BoxActionFromClientToCompanyUpdateDto dto)
        {
            if (_boxActionAppService.EditFromClientToCompany(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditFromCompanyToCompany(BoxActionFromCompanyToCompanyUpdateDto dto)
        {
            if (_boxActionAppService.EditFromCompanyToCompany(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult EditFromClientToPublicExpenes(BoxActionFromClientToPublicExpenesUpdateDto dto)
        {
            if (_boxActionAppService.EditExpenseFromClientToPublic(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult EditFromClientToPublicIncome(BoxActionFromClientToPublicIncomeUpdateDto dto)
        {
            if (_boxActionAppService.EditReceiveFromPublicToClient(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult EditFromCompanyToPublicExpenes(BoxActionFromCompanyToPublicExpenesUpdateDto dto)
        {
            if (_boxActionAppService.EditExpenseFromCompanyToPublic(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult EditFromCompanyToPublicIncome(BoxActionFromCompanyToPublicIncomeUpdateDto dto)
        {
            if (_boxActionAppService.EditReceiveFromPublicToCompany(dto))
                _success = true;
            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }
        #endregion

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