using BWR.Application.Interfaces.Client;
using BWR.Application.Interfaces.Setting;
using BWR.Application.Interfaces.Transaction;
using BWR.Application.Interfaces.Treasury;
using BWR.Domain.Model.Settings;
using PagedList;
using System;
using System.EnterpriseServices;
using System.Linq;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers.Transaction
{
    public class TransactionController : Controller
    {
        private readonly ITransactionAppService _transactionAppService;
        private readonly IAttachmentAppService _attachmentAppService;
        private readonly IClientAttatchmentAppService _clientAttatchmentAppService;
        private readonly ITreasuryAppService _treasuryAppService;
        private string _message;
        private bool _success;

        public TransactionController(ITransactionAppService transactionAppService,
            IClientAttatchmentAppService clientAttatchmentAppService,
            IAttachmentAppService attachmentAppService,
            ITreasuryAppService treasuryAppService)
        {
            _transactionAppService = transactionAppService;
            _clientAttatchmentAppService = clientAttatchmentAppService;
            _attachmentAppService = attachmentAppService;
            _treasuryAppService = treasuryAppService;
            _message = "";
            _success = false;
        }

        public ActionResult TransactionDontDileverd(int? companyId, DateTime? from, DateTime? to)
        {
            if (!CheckTreasury())
                return RedirectToAction("NoTreasury", "Home");

            var transactions = _transactionAppService.GetTransactionDontDileverd(from, to);
            return View(transactions);
        }

        [HttpGet]
        public ActionResult DileveredTransactions(int? companyId, DateTime? from, DateTime? to, int? page)
        {
            var transactions = _transactionAppService.GetDeliveredTransactions(companyId, from, to);

            var data = transactions.OrderBy(c => c.Id).ToPagedList(page ?? 1, 10);

            return View(data);
        }


        [HttpGet]
        public ActionResult DileverdTransaction(int transactionId)
        {
            if (!CheckTreasury())
                return RedirectToAction("NoTreasury", "Home");

            var transaction = _transactionAppService.GetById(transactionId);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            if ((bool)transaction.Deliverd)
            {
                return HttpNotFound();
            }
            ViewData["Attachments"] = new SelectList(_attachmentAppService.GetForDropdown(""), "Id", "Name");
            ViewBag.ClientAttachment = _clientAttatchmentAppService.GetAll().Where(c => c.ClientId == transaction.ReciverClientId);

            return View(transaction);
        }
        
        [HttpPost]
        public ActionResult DileverTransaction(int transactionId)
        {
            try
            {
                var transactionDto = _transactionAppService.DileverTransaction(transactionId);
                if (transactionDto != null)
                    return RedirectToAction("TransactionDontDileverd");
                else
                {
                    ModelState.AddModelError("", "حدثت مشكلة اثناء الحفظ");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حدثت مشكلة اثناء الحفظ");
            }

            return View();
        }
        //[HttpGet]
        //public ActionResult IncomeTransactionStatementDetailed(int? snederCompanyId, TypeOfPay typeOfPay, int? reciverId, int? senderClientId, int? coinId, TransactionStatus transactionStatus, DateTime? from, DateTime? to, bool? isDelivered)
        //{

        //    return Json("");
        //}

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