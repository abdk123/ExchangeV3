﻿using BWR.Application.AppServices.Common;
using BWR.Application.Common;
using BWR.Application.Interfaces;
using BWR.Application.Interfaces.Client;
using BWR.Application.Interfaces.Company;
using BWR.Application.Interfaces.Setting;
using BWR.Domain.Model.Settings;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Dynamic;
using BWR.Domain.Model.Companies;
using DataTables.Mvc;
using System.Collections.Generic;
using BWR.Infrastructure.Exceptions;
using BWR.Application.Interfaces.Transaction;

namespace Bwr.WebApp.Controllers.Common
{
    public class StatementController : Controller
    {
        private readonly ICoinAppService _coinAppService;
        private readonly ICompanyAppService _companyAppService;
        private readonly IClientAppService _clientAppService;
        private readonly ICountryAppService _countryAppService;
        private readonly IStatementAppService _statementAppService;
        private readonly IClientCashFlowAppService _clientCashFlowAppService;
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IInnerTransactionAppService _innerTransactionAppService;

        public StatementController(
            ICompanyAppService companyAppService,
            IClientAppService clientAppService,
            ICoinAppService coinAppService,
            ICountryAppService countryAppService,
            IStatementAppService statementAppService,
            IClientCashFlowAppService clientCashFlowAppService,
            IInnerTransactionAppService innerTransactionAppService,
            IUnitOfWork<MainContext> unitOfWork)
        {
            _clientAppService = clientAppService;
            _companyAppService = companyAppService;
            _coinAppService = coinAppService;
            _countryAppService = countryAppService;
            _statementAppService = statementAppService;
            _clientCashFlowAppService = clientCashFlowAppService;
            _unitOfWork = unitOfWork;
            _innerTransactionAppService = innerTransactionAppService;
        }

        // GET: Statement
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult OuterTransactions()
        {
            return PartialView("_OuterTransactions");

        }
        [HttpGet]
        public ActionResult ClientStoped()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Clering()
        {
            return PartialView("_Clering");
        }
        [HttpGet]
        public ActionResult Clearing(int coinId, DateTime? from, DateTime? to, IncomeOrOutCame incomeOrOutCame, ClearingAccountType fromAccountType, int? fromAccountId, ClearingAccountType toAccountType, int? toAccountId)
        {
            var list = this._statementAppService.GetClearing(coinId, incomeOrOutCame, from, to, fromAccountType, fromAccountId, toAccountType, toAccountId);
            return View(list);
        }

        [HttpGet]
        public ActionResult DileveredTransactions()
        {
            ViewBag.Companies = _companyAppService.GetForDropdown("");
            return PartialView("_DileveredTransactions");
        }

        [HttpGet]
        public ActionResult ClientsAndCompaniesBalances()
        {
            return PartialView("_ClientsAndCompaniesBalances");
        }


        [HttpGet]
        public ActionResult AllBalances(int coinId, DateTime? to)
        {
            ViewBag.CoinId = coinId;
            ViewBag.ToDate = to;
            return View();
        }

        [HttpGet]
        public ActionResult Conclusion()
        {
            return PartialView("_Conclusion");
        }

        [HttpGet]
        public ActionResult GetClientsAndCompaniesBalances(int coinId, DateTime to)
        {
            var balances = _statementAppService.GetAllBalances(coinId, to);
            return Json(new { Balances = balances }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult GetConclusion(int coinId, DateTime to)
        {
            var conclusion = _statementAppService.GetConclusion(coinId, to);
            return Json(new { Conclusion = conclusion }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult IncmeOuteComeView()
        {
            ViewBag.Coins = new SelectList(_unitOfWork.GenericRepository<Coin>().GetAll(), "Id", "Name");
            var coins = _unitOfWork.GenericRepository<BWR.Domain.Model.Clients.Client>().GetAll();
            ViewBag.Agent = new SelectList(coins, "Id", "FullName", coins.FirstOrDefault().Id);
            ViewBag.Companies = new SelectList(_unitOfWork.GenericRepository<BWR.Domain.Model.Companies.Company>().GetAll(), "Id", "Name");
            ViewBag.Expenses = new SelectList(_unitOfWork.GenericRepository<PublicExpense>().GetAll(), "Id", "Name");
            ViewBag.Income = new SelectList(_unitOfWork.GenericRepository<PublicIncome>().GetAll(), "Id", "Name");
            return View();
        }
        [HttpPost]
        public ActionResult IncomeOutCome([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int generealType, int coinId, PaymentsTypeEnum paymentsTypeEnum, DateTime? from, DateTime? to, int? PaymentsTypeEntityId)
        {
            DataTablesDto dto;
            if (generealType == -1)
            {
                dto= this._statementAppService.GetPayment(requestModel.Draw, requestModel.Start, requestModel.Length, coinId, paymentsTypeEnum, from, to, PaymentsTypeEntityId);
            }
            else
            {   
                dto= this._statementAppService.GetIncme(requestModel.Draw, requestModel.Start, requestModel.Length, coinId, paymentsTypeEnum, from, to, PaymentsTypeEntityId);
                
            }
            
            return Json(dto,JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult IncomeTransactionStatementDetailed()
        {
            var companies = _unitOfWork.GenericRepository<BWR.Domain.Model.Companies.Company>().GetAll();
            var agents = _unitOfWork.GenericRepository<BWR.Domain.Model.Clients.Client>().FindBy(c => c.ClientType == BWR.Domain.Model.Clients.ClientType.Client).ToList();
            var coins = _unitOfWork.GenericRepository<BWR.Domain.Model.Settings.Coin>().GetAll().ToList();
            ViewBag.Companies = new SelectList(companies, "Id", "Name");
            ViewBag.Agents = new SelectList(agents, "Id", "FullName");
            ViewBag.Coins = new SelectList(coins, "Id", "Name");
            return View();
        }
        [HttpGet]
        public ActionResult CommissionReport()
        {
            var company = _unitOfWork.GenericRepository<BWR.Domain.Model.Companies.Company>().GetAll();
            var agnet = _unitOfWork.GenericRepository<BWR.Domain.Model.Clients.Client>().FindBy(c=>c.ClientType==BWR.Domain.Model.Clients.ClientType.Client);
            var coins = _unitOfWork.GenericRepository<Coin>().GetAll();
            var countries = _unitOfWork.GenericRepository<Country>().FindBy(c => c.MainCountryId == null).ToList();
            var ceities = _unitOfWork.GenericRepository<Country>().FindBy(c => c.MainCountryId != null).ToList();
            var unionCountries = new List<Country>();
            foreach (var item in countries)
            {
                unionCountries.Add(item);
                var c= ceities.Where(cc => cc.MainCountryId == item.Id);
                unionCountries.AddRange(c);
            }
            ViewBag.Companies = new SelectList(company, "Id", "Name");
            ViewBag.Agent = new SelectList(agnet, "Id", "FullName");
            ViewBag.Coin = new SelectList(coins, "Id", "Name");
            ViewBag.Countries = new SelectList(unionCountries, "Id", "Name");
            return View();
        }
        [HttpPost]
        public ActionResult CommissionReport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int? coinId, DateTime? from, DateTime? to, int? countryId, int? companyId, int? agentId)
        {
            try
            {
                
                return Json(_statementAppService.CommissionReport(requestModel.Draw, requestModel.Start, requestModel.Length, coinId, from, to, companyId, agentId, countryId));
            }
            catch(Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return Json("");
        }
        [HttpGet]
        public ActionResult IncmoeTransactionGross()
        {
            var companies = _unitOfWork.GenericRepository<BWR.Domain.Model.Companies.Company>().GetAll();
            ViewBag.companyies = new SelectList(companies, "Id", "Name");
            return View();
        }
        [HttpPost]
        public ActionResult IncmoeTransactionGross([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int?companyId,DateTime? from , DateTime? to)
        {

            return Json(_innerTransactionAppService.IncmoeTransactionGross(requestModel.Draw,requestModel.Start,requestModel.Length,companyId,from,to));
        }

    }
}