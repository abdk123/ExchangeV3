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

        public StatementController(
            ICompanyAppService companyAppService,
            IClientAppService clientAppService,
            ICoinAppService coinAppService,
            ICountryAppService countryAppService,
            IStatementAppService statementAppService,
            IClientCashFlowAppService clientCashFlowAppService,
            IUnitOfWork<MainContext> unitOfWork)
        {
            _clientAppService = clientAppService;
            _companyAppService = companyAppService;
            _coinAppService = coinAppService;
            _countryAppService = countryAppService;
            _statementAppService = statementAppService;
            _clientCashFlowAppService = clientCashFlowAppService;
            _unitOfWork = unitOfWork;
        }

        // GET: Statement
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult OuterTransactions()
        {
            //var coins = _coinAppService.GetForDropdown("");
            //var agents = _clientAppService.Get(x => x.ClientType == ClientType.Client);
            //var clients = _clientAppService.Get(x => x.ClientType == ClientType.Normal);
            //var companies = _clientAppService.GetAll();
            //var countries = _countryAppService.GetForDropdown("");

            //ViewBag.Coins = coins;
            //ViewBag.Agents = agents;
            //ViewBag.Clients = clients;
            //ViewBag.Companies = companies;
            //ViewBag.Countries = countries;

            return PartialView("_OuterTransactions");

        }
        [HttpGet]
        public ActionResult ClientStoped()
        {
            return View();
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
        [HttpGet]
        public ActionResult IncomeOutCome(int generealType, int coinId, PaymentsTypeEnum paymentsTypeEnum, DateTime? from, DateTime? to, int? PaymentsTypeEntityId)
        {
            if (generealType == -1)
            {
                var info = this._statementAppService.GetPayment(coinId, paymentsTypeEnum, from, to, PaymentsTypeEntityId).ToList();
                return Json(info, JsonRequestBehavior.AllowGet);
            }
            return Json(null);
        }

    }
}