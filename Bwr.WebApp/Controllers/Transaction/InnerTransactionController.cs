﻿using Bwr.WebApp.Models.Security;
using BWR.Application.Common;
using BWR.Application.Dtos.Transaction.InnerTransaction;
using BWR.Application.Interfaces.Transaction;
using BWR.Application.Interfaces.Treasury;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Transactions;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;
using BWR.ShareKernel.Permisions;
using DataTables.Mvc;
using System;
using System.Web.Mvc;
using System.Linq;

namespace Bwr.WebApp.Controllers.Transaction
{
    [Authorize]
    public class InnerTransactionController : Controller
    {
        private readonly IInnerTransactionAppService _innerTransactionAppService;
        private readonly ITreasuryAppService _treasuryAppService;
        private readonly IUnitOfWork<MainContext> _unitOfWork;

        public InnerTransactionController(IInnerTransactionAppService innerTransactionAppService,
            IUnitOfWork<MainContext> _unitOfWork
            , ITreasuryAppService treasuryAppService)
        {
            _innerTransactionAppService = innerTransactionAppService;
            _treasuryAppService = treasuryAppService;
            this._unitOfWork = _unitOfWork;
        }

        // GET: InnerTransaction
        public ActionResult Index()
        {
            if (!CheckTreasury())
                return RedirectToAction("NoTreasury", "Home");

            var innerTransactionInitialDto = _innerTransactionAppService.InitialInputData();
            ViewBag.Companies = new SelectList(innerTransactionInitialDto.Companies, "Id", "Name");
            ViewBag.Coin = new SelectList(innerTransactionInitialDto.Coins, "Id", "Name");

            //ViewBag.Clients = new SelectList(innerTransactionInitialDto.Clients, "Id", "FullName", null, null,innerTransactionInitialDto.Clients.Where(c => c.IsEnabled == false));
            ViewData["Clients"] = innerTransactionInitialDto.Clients.ToList();
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
            if (PermissionHelper.CheckPermission(AppPermision.Action_OuterTransaction_EditInnerTransaction))
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
        [HttpPost]
        public ActionResult InnerTransactionStatementDetailed([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int? reciverCompanyId, TypeOfPay typeOfPay, int? reciverId, int? senderCompanyId, int? senderClientId, int? coinId, TransactionStatus transactionStatus, DateTime? from, DateTime? to, bool? isDelivered)
        {
            DataTablesDto dto = _innerTransactionAppService.InnerTransactionStatementDetailed(requestModel.Draw, requestModel.Start, requestModel.Length, reciverCompanyId, typeOfPay, reciverId, senderCompanyId, senderClientId, coinId, transactionStatus, from, to, isDelivered);
            return Json(dto, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult TransactionDontDileverd()
        {
            var companies = _unitOfWork.GenericRepository<BWR.Domain.Model.Companies.Company>().GetAll();
            var coins = _unitOfWork.GenericRepository<Coin>().GetAll();
            ViewBag.companies = new SelectList(companies, "Id", "Name");
            ViewBag.coins = new SelectList(coins, "Id", "Name");
            return View();
        }
        [HttpPost]
        public ActionResult TransactionDontDileverd([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int? clientId, int? companyId, int? coinId, TransactionStatus transactionStatus, DateTime? from, DateTime? to)
        {
            return Json(_innerTransactionAppService.TransactionDontDileverd(requestModel.Draw, requestModel.Start, requestModel.Length, transactionStatus, clientId, companyId, coinId, from, to));
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