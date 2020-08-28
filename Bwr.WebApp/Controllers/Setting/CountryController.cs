using BWR.Application.Dtos.Setting.Country;
using BWR.Application.Interfaces.Setting;
using BWR.ShareKernel.Exceptions;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using Bwr.WebApp.Extensions;
using BWR.Infrastructure.Exceptions;
using DataTables.Mvc;
using BWR.Domain.Model.Settings;
using Bwr.WebApp.Models.Security;
using System;
using BWR.Application.Dtos.Setting.Provinces;

namespace Bwr.WebApp.Controllers
{
    public class CountryController : Controller
    {
        private readonly ICountryAppService _countryAppService;
        private readonly IProvinceAppService _provinceAppService;
        private string _message;
        private bool _success;

        public CountryController(ICountryAppService countryAppService, IProvinceAppService provinceAppService)
        {
            _countryAppService = countryAppService;
            _provinceAppService = provinceAppService;
            _message = "";
            _success = false;
        }

        public ActionResult Index()
        {
            return View();
        }

        #region Country

        public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var countries = _countryAppService.GetAll().AsQueryable();

                var totalCount = countries.Count();

                // searching and sorting
                countries = SearchAndSort(requestModel, countries);
                var filteredCount = countries.Count();

                // Paging
                countries = countries.Skip(requestModel.Start).Take(requestModel.Length);

                var dataTablesResponse = new DataTablesResponse(requestModel.Draw, countries.ToList(), filteredCount, totalCount);
                return Json(dataTablesResponse, JsonRequestBehavior.AllowGet);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json(new { Success = false, Message = _message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAllWithProvinces()
        {
            var countries = _countryAppService.GetCountriesAndProvinces();

            return Json(new { Countries = countries }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSpecificCountry(string name, int? id)
        {

            try
            {
                var country = new CountryForDropdownDto();
                if (id != null)
                    country = _countryAppService.GetForDropdown(name).FirstOrDefault(x => x.Id != id);
                else
                    country = _countryAppService.GetForDropdown(name).FirstOrDefault();

                return Json(new { Success = true, Country = country }, JsonRequestBehavior.AllowGet);
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
            return View("_CreateCountry");
        }

        [HttpPost]
        public ActionResult Create(CountryInsertDto dto)
        {
            var exist = false;

            if (_countryAppService.CheckIfExist(dto.Name, 0))
            {
                _success = false;
                _message = "يوجد بلد بنفس الاسم";
                exist = true;
            }
            else
            {
                var countryDto = _countryAppService.Insert(dto);
                if (countryDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء إضافة بيانات البلد";
                }
            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var dto = _countryAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditCountry", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Edit(CountryUpdateDto dto)
        {
            var exist = false;

            if (_countryAppService.CheckIfExist(dto.Name, dto.Id))
            {
                _success = false;
                _message = "يوجد بلد بنفس الاسم";
                exist = true;
            }
            else
            {
                var countryDto = _countryAppService.Update(dto);
                if (countryDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء تعديل بيانات البلد";
                }
            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var dto = _countryAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeleteCountry", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Delete(CountryDto dto)
        {
            _countryAppService.Delete(dto.Id.Value);

            return Content("success");
        }

        public ActionResult Detail(int id)
        {
            var dto = _countryAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DetailsCountry", dto);

            return View(dto);
        }

        #endregion

        #region Province

        [HttpGet]
        public ActionResult GetProvincesForSpecificCountry(int countryId)
        {
            var provincesDto = _provinceAppService.GetProvinceForSpecificCountry(countryId);
            return Json(new { Provinces = provincesDto }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddProvinceForSpecificCountry(ProvinceInsertDto province)
        {
            var provinceDto = _provinceAppService.Insert(province);
            if (provinceDto != null)
                _success = true;
            else
            {
                _success = false;
                _message = "حدثت مشكلة اثناء حفظ بيانات المحافظة";
            }

            return Json(new { Success = _success, Province = provinceDto }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditProvinceForSpecificCountry(ProvinceUpdateDto province)
        {
            var provinceDto = _provinceAppService.Update(province);
            if (provinceDto != null)
                _success = true;
            else
            {
                _success = false;
                _message = "حدثت مشكلة اثناء تعديل بيانات المحافظة";
            }

            return Json(new { Success = _success, Province = provinceDto }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteProvinceForSpecificCountry(int provinceId)
        {
            _provinceAppService.Delete(provinceId);

            return Json(new { Success = true}, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Helper Method

        private IQueryable<CountryDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<CountryDto> query)
        {
            // Apply filters
            if (requestModel.Search.Value != string.Empty)
            {
                var value = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Name.Contains(value)
                                   //p.Address.Contains(value) ||
                                   //p.Phone.Contains(value) ||
                                   //p.Mobile.Contains(value)

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