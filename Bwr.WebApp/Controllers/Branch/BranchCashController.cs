using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BWR.Application.Dtos.Branch;
using BWR.Application.Interfaces.Branch;
using BWR.Domain.Model.Branches;

namespace Bwr.WebApp.Controllers.Setting
{
    public class BranchCashController : Controller
    {
        private readonly IBranchCashAppService _branchCashAppService;
        private string _message;
        private bool _success;

        public BranchCashController(IBranchCashAppService branchCashAppService)
        {
            _branchCashAppService = branchCashAppService;
            _message = "";
            _success = false;
        }
        // GET: BranchCash
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Get()
        {
            var branchCaches = _branchCashAppService.GetAll();

            return Json(new { data = branchCaches }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetForBranch(int? id)
        {
            if (id == null)
                id = BranchHelper.Id;

            var branchCaches = _branchCashAppService.GetForSpecificBranch(id.Value);

            return Json(branchCaches);
        }

        public bool ChecekIfTherIsMaincoin()
        {
            return _branchCashAppService.GetAll().Any(c => c.IsMainCoin);
        }

        public bool IsMaincoin(int coinId)
        {
            return _branchCashAppService.GetAll().Any(c => c.CoinId == coinId && c.IsMainCoin);
        }

        public ActionResult GetCoinExchange(int coinId)
        {
            return Json(_branchCashAppService.GetAll().Where(c => c.CoinId == coinId).FirstOrDefault(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var dto = _branchCashAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditBranchCash", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Edit(BranchCashUpdateDto dto)
        {
            var branchCashDto = _branchCashAppService.Update(dto);
            if (branchCashDto != null)
                _success = true;
            else
            {
                _success = false;
                _message = "حدثت مشكلة اثناء تعديل البيانات ";
            }

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetActualBalance(int coinId)
        {
            int branchId = BranchHelper.Id;
            var balance = _branchCashAppService.GetActualBalance(coinId, branchId);
            return Json(balance, JsonRequestBehavior.AllowGet);
        }
       
    }
}