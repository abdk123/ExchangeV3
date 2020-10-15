using Bwr.WebApp.Models.Security;
using BWR.Application.Dtos.Transaction.InnerTransaction;
using BWR.Application.Interfaces.Transaction;
using BWR.Application.Interfaces.Treasury;
using BWR.ShareKernel.Permisions;
using System;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers.Transaction
{
    [Authorize]
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

        public ActionResult EditInnerTransaction(int id)
        {
            var innerTransactionInitialDto = _innerTransactionAppService.InitialInputData();
            ViewBag.Companies = new SelectList(innerTransactionInitialDto.Companies, "Id", "Name");
            ViewBag.Coin = new SelectList(innerTransactionInitialDto.Coins, "Id", "Name");
            ViewBag.Clients = new SelectList(innerTransactionInitialDto.Clients, "Id", "FullName");
            ViewData["NormalClient"] = innerTransactionInitialDto.NormalClients;
            var innerTransaction = _innerTransactionAppService.GetForEdit(id);

            return View(innerTransaction);
        }

        public ActionResult InnerTransactionDetails(int transactionId)
        {
            if(PermissionHelper.CheckPermission(AppPermision.Action_OuterTransaction_EditInnerTransaction))
                return RedirectToAction("EditInnerTransaction", "InnerTransaction", new { id = transactionId });

            var innerTransactionInitialDto = _innerTransactionAppService.InitialInputData();
            ViewBag.Companies = new SelectList(innerTransactionInitialDto.Companies, "Id", "Name");
            ViewBag.Coin = new SelectList(innerTransactionInitialDto.Coins, "Id", "Name");
            ViewBag.Clients = new SelectList(innerTransactionInitialDto.Clients, "Id", "FullName");
            ViewData["NormalClient"] = innerTransactionInitialDto.NormalClients;
            var innerTransaction = _innerTransactionAppService.GetForEdit(transactionId);

            return View(innerTransaction);
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

        [HttpPost]
        public ActionResult SaveInnerTransactionForEdit(InnerTransactionUpdateDto input)
        {
            try
            {
                bool transactionsSaved = _innerTransactionAppService.EditInnerTransaction(input);

                return Json(true);
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