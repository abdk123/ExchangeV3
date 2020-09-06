using BWR.Application.Dtos.Branch;
using BWR.Application.Interfaces.Branch;
using BWR.Application.Interfaces.Client;
using BWR.Application.Interfaces.Common;
using BWR.Application.Interfaces.Setting;
using BWR.Application.Interfaces.Transaction;
using BWR.Application.Interfaces.Treasury;
using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers.Setting
{
    public class ExchangeController : Controller
    {
        private readonly IBranchCashAppService _branchCashAppService;
        private readonly ITreasuryAppService _treasuryAppService;
        private readonly ITreasuryCashAppService _treasuryCashAppService;
        private readonly IOuterTransactionAppService _outerTransactionAppService;
        private readonly IExchangeAppService _exchangeAppService;

        public ExchangeController(IBranchCashAppService branchCashAppService, 
            ITreasuryAppService treasuryAppService,
            ITreasuryCashAppService treasuryCashAppService,
            IExchangeAppService exchangeAppService,
            IOuterTransactionAppService outerTransactionAppService)
        {
            _branchCashAppService = branchCashAppService;
            _treasuryAppService = treasuryAppService;
            _treasuryCashAppService = treasuryCashAppService;
            _outerTransactionAppService = outerTransactionAppService;
            _exchangeAppService = exchangeAppService;
        }
        // GET: BranchCash
        public ActionResult Index()
        {
            var thereIsMainBranchCash = _branchCashAppService.GetAll().Any(x => x.IsMainCoin == true);
            if (thereIsMainBranchCash)
                return View();
            else
                return View("MainCoinIsRequired");
        }

        public ActionResult ExchangeTemplate()
        {
            if (!CheckTreasury())
                return RedirectToAction("NoTreasury", "Home");

            var initialInputs = _outerTransactionAppService.InitialInputData();
            var coins = _branchCashAppService.GetAll().Select(x => new { Id = x.CoinId, Name = x.Coin.Name });

            ViewBag.FirstCoins = new SelectList(coins, "Id", "Name");
            ViewBag.SecondCoins = new SelectList(coins, "Id", "Name");

            ViewData["Clients"] = initialInputs.Clients;
            ViewBag.Agents = new SelectList(initialInputs.Agents, "Id", "FullName");
            ViewBag.Companies = new SelectList(initialInputs.Companies, "Id", "Name");
            return View();
        }

        public ActionResult MainCoinIsRequired()
        {
            return View();
        }

        public ActionResult Get()
        {
            var branchCaches = _branchCashAppService.GetAll().Where(x => !x.IsMainCoin);

            return Json(new { data = branchCaches }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(IList<BranchCashUpdateDto> branchCashes)
        {
            var message = "";
            var success = true;
            try
            {
                _branchCashAppService.UpdateAll(branchCashes);
            }
            catch (Exception ex)
            {
                success = false;
                message = "حدثت مشكلة اثناء تعديل بيانات العميل";
            }

            return Json(new { Success = success, Message = message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExchangeForClient(int clientId, int sellingCoinId, int purchasingCoinId, decimal firstAmount)
        {
            return Json(_exchangeAppService.ExchangeForClient(clientId, sellingCoinId, purchasingCoinId, firstAmount));
        }

        public ActionResult ExchangeForCompany(int companyId, int sellingCoinId, int purchasingCoinId, decimal firstAmount)
        {
            return Json(_exchangeAppService.ExchangeForCompany(companyId, sellingCoinId, purchasingCoinId, firstAmount));
        }

        public ActionResult ExchangeForBranch(int sellingCoinId, int purchasingCoinId, decimal firstAmount)
        {
            return Json(_exchangeAppService.ExchangeForBranch(sellingCoinId, purchasingCoinId, firstAmount));
        }

        public decimal CalcForFirstCoin(int sellingCoinId, int purchasingCoinId, decimal amountFromFirstCoin)
        {
            return _exchangeAppService.CalcForFirstCoin(sellingCoinId, purchasingCoinId, amountFromFirstCoin);
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

        private int GetTreasuryId()
        {
            int treasuryId = 0;
            var currentTreasury = Session["CurrentTreasury"];
            if (currentTreasury != null && currentTreasury.ToString() != "0")
                treasuryId = (int)currentTreasury;

            return treasuryId;
        }

    }
}