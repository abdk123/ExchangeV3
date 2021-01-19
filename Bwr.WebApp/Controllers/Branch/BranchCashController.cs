﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BWR.Application.Dtos.Branch;
using BWR.Application.Interfaces.Branch;
using BWR.Application.Interfaces.BranchCashFlow;
using BWR.Domain.Model.Branches;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;
using DataTables.Mvc;

namespace Bwr.WebApp.Controllers.Setting
{
    [Authorize]
    public class BranchCashController : Controller
    {
        private readonly IBranchCashAppService _branchCashAppService;
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private string _message;
        private bool _success;

        public BranchCashController(IBranchCashAppService branchCashAppService, IUnitOfWork<MainContext> unitOfWork)
        {
            _branchCashAppService = branchCashAppService;
            _message = "";
            _success = false;
            this._unitOfWork = unitOfWork;
        }
        // GET: BranchCash
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var branchCaches = _branchCashAppService.GetAll();
            var totalCount = branchCaches.Count();
            var filteredCount = branchCaches.Count();
            foreach (var item in branchCaches)
            {
                item.Total = this._unitOfWork.GenericRepository<BranchCashFlow>().FindBy(c => c.CoinId == item.CoinId).Sum(c => c.Amount) + item.InitialBalance;
            }
            var dataTablesResponse = new DataTablesResponse(requestModel.Draw, branchCaches.ToList(), filteredCount, totalCount);
            return Json(dataTablesResponse, JsonRequestBehavior.AllowGet);
            //return Json(new { data = branchCaches }, JsonRequestBehavior.AllowGet);
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

        //private IQueryable<BranchCashDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<BranchCashDto> query)
        //{
        //    // Apply filters
        //    if (requestModel.Search.Value != string.Empty)
        //    {
        //        var value = requestModel.Search.Value.Trim();
        //        query = query.Where(p => p.Name.Contains(value) ||
        //                           p.Code.Contains(value)

        //                           );
        //    }

        //    var filteredCount = query.Count();

        //    // Sort
        //    var sortedColumns = requestModel.Columns.GetSortedColumns();
        //    var orderByString = string.Empty;

        //    foreach (var column in sortedColumns)
        //    {
        //        orderByString += orderByString != string.Empty ? "," : "";
        //        orderByString += (column.Data) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
        //    }

        //    query = query.OrderBy(orderByString == string.Empty ? "BarCode asc" : orderByString);

        //    return query;
        //}

    }
}