using BWR.Application.Dtos.Transaction.InnerTransaction;
using BWR.Application.Interfaces.Client;
using BWR.Application.Interfaces.Setting;
using BWR.Application.Interfaces.Transaction;
using BWR.Application.Interfaces.Treasury;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Transactions;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;
using PagedList;
using System;
using System.EnterpriseServices;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers.Transaction
{
    public class TransactionController : Controller
    {
        private readonly ITransactionAppService _transactionAppService;
        private readonly IInnerTransactionAppService _innerTransactionAppService;
        private readonly IAttachmentAppService _attachmentAppService;
        private readonly IClientAttatchmentAppService _clientAttatchmentAppService;
        private readonly ITreasuryAppService _treasuryAppService;
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private string _message;
        private bool _success;

        public TransactionController(ITransactionAppService transactionAppService,
            IClientAttatchmentAppService clientAttatchmentAppService,
            IAttachmentAppService attachmentAppService,
            ITreasuryAppService treasuryAppService,
            IUnitOfWork<MainContext> unitOfWork,
            IInnerTransactionAppService innerTransactionAppService)
        {
            _transactionAppService = transactionAppService;
            _clientAttatchmentAppService = clientAttatchmentAppService;
            _attachmentAppService = attachmentAppService;
            _treasuryAppService = treasuryAppService;
            _message = "";
            _success = false;
            _unitOfWork = unitOfWork;
            _innerTransactionAppService = innerTransactionAppService;
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
        [HttpGet]
        public ActionResult EditTransactionCollection(int collectionId)
        {
            var innerTransactionInitialDto = _innerTransactionAppService.InitialInputData();
            ViewBag.Companies = new SelectList(innerTransactionInitialDto.Companies, "Id", "Name");
            ViewBag.Coin = new SelectList(innerTransactionInitialDto.Coins, "Id", "Name");
            ViewBag.Clients = new SelectList(innerTransactionInitialDto.Clients, "Id", "FullName");
            ViewData["NormalClient"] = innerTransactionInitialDto.NormalClients;
            return View();
        }
        [HttpGet]
        public ActionResult GetTransactionCollectionById(int collectionId)
        {
            var incomeTransactionCollection = _unitOfWork.GenericRepository<IncomeTransactionCollection>().FindBy(c => c.Id == collectionId, "Company", "Transactions.SenderClient", "Transactions.SenderCompany ", "Transactions.ReciverClient", "Transactions.ReceiverCompany", "Transactions.Coin", "Transactions.Country").FirstOrDefault();
            IncomeTransactionCollectionDetailsDto incomeTransactionCollectionDetailsDto = new IncomeTransactionCollectionDetailsDto()
            {
                Id = incomeTransactionCollection.Id,
                CompanyId = incomeTransactionCollection.CompnayId,
                //Date = incomeTransactionCollection.Transactions.First().MoenyActions.First().Date.ToString("dd/MM/yyyy", new CultureInfo("ar-AE")),
                Date = incomeTransactionCollection.Transactions.First().MoenyActions.First().Date,
                Note = incomeTransactionCollection.Note
            };
            foreach (var item in incomeTransactionCollection.Transactions)
            {
                IncomeTransactionDetailsDto incomeTransactionDetailsDto = new IncomeTransactionDetailsDto()
                {
                    Amount = item.Amount,
                    CoinId = item.CoinId,
                    SenderClientId = item.SenderClientId,
                    OurComission = item.OurComission,
                    TypeOfPay = item.TypeOfPay,
                    ReceiverCompanyId = item.ReceiverCompanyId,
                    ReceiverCompanyComission = item.ReceiverCompanyComission,
                    ReciverClientId = item.ReciverClientId,
                    ReciverClientCommission = item.ReciverClientCommission,
                    SenderCompanyId =item.SenderCompanyId,
                    SenderCompanyComission = item.SenderCompanyComission
                };
                incomeTransactionCollectionDetailsDto.IncomeTransactionDetails.Add(incomeTransactionDetailsDto);
            }

            return Json(incomeTransactionCollectionDetailsDto,JsonRequestBehavior.AllowGet);
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