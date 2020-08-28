using BWR.Application.Dtos.Setting.Coin;
using BWR.Application.Interfaces.Setting;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using DataTables.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers.Setting
{
    public class CoinController : Controller
    {
        private readonly ICoinAppService _coinAppService;
        private string _message;
        private bool _success;

        public CoinController(ICoinAppService coinAppService)
        {
            _coinAppService = coinAppService;
            _message = "";
            _success = false;
        }

        public ActionResult Index()
        {
            return View();
        }

        #region Coin

        public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var coins = _coinAppService.GetAll().AsQueryable();

                var totalCount = coins.Count();

                // searching and sorting
                coins = SearchAndSort(requestModel, coins);
                var filteredCount = coins.Count();

                // Paging
                coins = coins.Skip(requestModel.Start).Take(requestModel.Length);

                var dataTablesResponse = new DataTablesResponse(requestModel.Draw, coins.ToList(), filteredCount, totalCount);
                return Json(dataTablesResponse, JsonRequestBehavior.AllowGet);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json(new { Success = false, Message = _message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCoinsForDropdown(string term)
        {
            var coins = _coinAppService.GetForDropdown(term);

            return Json(new { Coins = coins }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSpecificCoin(string name, int? id)
        {

            try
            {
                var coin = new CoinForDropdownDto();
                if (id != null)
                    coin = _coinAppService.GetForDropdown(name).FirstOrDefault(x => x.Id != id);
                else
                    coin = _coinAppService.GetForDropdown(name).FirstOrDefault();

                return Json(new { Success = true, Coin = coin }, JsonRequestBehavior.AllowGet);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json(new { Success = false, Message = _message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View("_CreateCoin");
        }

        [HttpPost]
        public ActionResult Create(CoinInsertDto dto)
        {
            var exist = false;

            if (_coinAppService.CheckIfExist(dto.Name, 0))
            {
                _success = false;
                _message = "توجد عملة بنفس الاسم";
                exist = true;
            }
            else
            {
                var coinDto = _coinAppService.Insert(dto);
                if (coinDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء إضافة بيانات العملة";
                }
            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var dto = _coinAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditCoin", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Edit(CoinUpdateDto dto)
        {
            var exist = false;

            if (_coinAppService.CheckIfExist(dto.Name, dto.Id.Value))
            {
                _success = false;
                _message = "توجد عملة بنفس الاسم";
                exist = true;
            }
            else
            {
                var coinDto = _coinAppService.Update(dto);
                if (coinDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء تعديل بيانات العملة";
                }
            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var dto = _coinAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeleteCoin", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Delete(CoinDto dto)
        {
            _coinAppService.Delete(dto.Id.Value);

            return Content("success");
        }

        public ActionResult Detail(int id)
        {
            var dto = _coinAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DetailsCoin", dto);

            return View(dto);
        }

        #endregion



        #region Helper Method

        private IQueryable<CoinDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<CoinDto> query)
        {
            // Apply filters
            if (requestModel.Search.Value != string.Empty)
            {
                var value = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Name.Contains(value) ||
                                   p.Code.Contains(value) 

                                   );
            }

            var filteredCount = query.Count();

            // Sort
            var sortedColumns = requestModel.Columns.GetSortedColumns();
            var orderByString = string.Empty;

            foreach (var column in sortedColumns)
            {
                orderByString += orderByString != string.Empty ? "," : "";
                orderByString += (column.Data) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
            }

            query = query.OrderBy(orderByString == string.Empty ? "BarCode asc" : orderByString);

            return query;
        }

        #endregion
    }
}