using BWR.Application.Dtos.Transaction.InnerTransaction;
using BWR.Application.Interfaces.Transaction;
using BWR.Application.Interfaces.Treasury;
using System;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers.Transaction
{
    public class InnerTransactionController : Controller
    {
        private readonly IInnerTransactionAppService _innerTransactionAppService;
        private readonly ITreasuryAppService _treasuryAppService;

        public InnerTransactionController(IInnerTransactionAppService innerTransactionAppService
            , ITreasuryAppService treasuryAppService)
        {
            _innerTransactionAppService = innerTransactionAppService;
            _treasuryAppService = treasuryAppService;
        }
        // GET: InnerTransaction
        public ActionResult Index()
        {
            if (!CheckTreasury())
                return RedirectToAction("NoTreasury", "Home");

            var innerTransactionInitialDto = _innerTransactionAppService.InitialInputData();
            ViewBag.Companies = new SelectList(innerTransactionInitialDto.Companies, "Id", "Name");
            ViewBag.Coin = new SelectList(innerTransactionInitialDto.Coins, "Id", "Name");
            ViewBag.Clients = new SelectList(innerTransactionInitialDto.Clients, "Id", "FullName");
            ViewData["NormalClient"] = innerTransactionInitialDto.NormalClients;
            return View();
        }

        public ActionResult InnerTransactionDetails(int transactionId)
        {
            var transaction = _innerTransactionAppService.GetById(transactionId);

            return View(transaction);
        }

        
        [HttpPost]
        public ActionResult SaveInnerTransactions(InnerTransactionInsertListDto input)
        {
            try
            {
                bool transactionsSaved = _innerTransactionAppService.SaveInnerTransactions(input);

                return Json(transactionsSaved);
            }
            catch (Exception ex)
            {
                return Json("error");
            }
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