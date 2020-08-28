using BWR.Application.Dtos.Setting.Country;
using BWR.Application.Interfaces.Setting;
using BWR.ShareKernel.Exceptions;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using BWR.Infrastructure.Exceptions;
using DataTables.Mvc;
using BWR.Application.Dtos.Setting.Provinces;
using System.Collections.Generic;
using System;

namespace Bwr.WebApp.Controllers
{
    public class ProvinceController : Controller
    {
        private readonly IProvinceAppService _provinceAppService;
        private readonly ICountryAppService _countryAppService;
        private readonly IList<CountryForDropdownDto> _countryForDropdownDtos;
        private string _message;

        public ProvinceController(ICountryAppService countryAppService,IProvinceAppService provinceAppService)
        {
            _provinceAppService = provinceAppService;
            _countryAppService = countryAppService;
            _countryForDropdownDtos = _countryAppService.GetForDropdown(string.Empty); 
            _message = "خطأ في النظام";
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var provinces = _provinceAppService.GetAll().AsQueryable();

                var totalCount = provinces.Count();

                // searching and sorting
                provinces = SearchAndSort(requestModel, provinces);
                var filteredCount = provinces.Count();

                // Paging
                provinces = provinces.Skip(requestModel.Start).Take(requestModel.Length);

                var dataTablesResponse = new DataTablesResponse(requestModel.Draw, provinces.ToList(), filteredCount, totalCount);
                return Json(dataTablesResponse, JsonRequestBehavior.AllowGet);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json(new { Success = false, Message = _message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSpecificProvince(string name, int? id)
        {

            try
            {
                var province = new ProvinceForDropdownDto();
                if (id != null)
                    province = _provinceAppService.GetForDropdown(name).FirstOrDefault(x => x.Id != id);
                else
                    province = _provinceAppService.GetForDropdown(name).FirstOrDefault();

                return Json(new { Success = true, Province = province }, JsonRequestBehavior.AllowGet);
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
            ViewBag.Countries = _countryForDropdownDtos;
            return View("_CreateProvince");
        }

        [HttpPost]
        public ActionResult Create(ProvinceInsertDto dto)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.Countries = _countryForDropdownDtos;
                return View("_CreateProvince", dto);
            }

            if (_provinceAppService.CheckIfExist(dto.Name, dto.MainCountryId.Value, 0))
            {
                ViewBag.Countries = _countryForDropdownDtos;
                ModelState.AddModelError("Name", "يوجد محافظة بنفس الاسم لنفس البلد");
                return View("_CreateProvince", dto);
            }

            var provinceDto = _provinceAppService.Insert(dto);
            if (provinceDto == null)
            {
                
                ModelState.AddModelError("ExceptionError", "حدثت مشكلة اثناء إضافة بيانات البلد");
                return View("_CreateProvince", dto);
            }
            return Content("success");
        }
        
        [HttpGet]
        public ActionResult Edit(int id)
        {
            ViewBag.Countries = _countryForDropdownDtos;
            var dto = _provinceAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditProvince", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Edit(ProvinceUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = _countryForDropdownDtos;
                return View("_EditProvince", dto);
            }

            if (_provinceAppService.CheckIfExist(dto.Name, dto.MainCountryId.Value, dto.Id))
            {
                ViewBag.Countries = _countryForDropdownDtos;
                ModelState.AddModelError("Name", "يوجد محافظة بنفس الاسم لنفس البلد");
                return View("_EditProvince", dto);
            }

            var provinceDto = _provinceAppService.Update(dto);
            if (provinceDto == null)
            {
                ModelState.AddModelError("ExceptionError", "حدثت مشكلة اثناء إضافة بيانات البلد");
                return View("_EditProvince", dto);
            }
            return Content("success");
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var dto = _provinceAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeleteProvince", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Delete(ProvinceDto dto)
        {
            _provinceAppService.Delete(dto.Id.Value);

            return Content("success");
        }

        public ActionResult Detail(int id)
        {
            var dto = _provinceAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DetailsProvince", dto);

            return View(dto);
        }

        private IQueryable<ProvinceDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<ProvinceDto> query)
        {
            // Apply filters
            if (requestModel.Search.Value != string.Empty)
            {
                var value = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Name.Contains(value) ||
                                   p.MainCountry != null ? p.MainCountry.Name.Contains(value) : false
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
    }
}