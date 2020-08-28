using BWR.Application.Dtos.Company;
using BWR.Application.Extensions;
using BWR.Application.Interfaces.Company;
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

namespace Bwr.WebApp.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ICompanyAppService _companyAppService;
        private readonly ICompanyCashAppService _companyCashAppService;
        private readonly ICompanyCountryAppService _companyCountryAppService;
        private readonly ICoinAppService _coinAppService;
        private string _message;
        private bool _success;

        public CompanyController(
            ICompanyAppService companyAppService,
            ICompanyCashAppService companyCashAppService,
            ICoinAppService coinAppService,
            ICompanyCountryAppService companyCountryAppService)
        {
            _companyAppService = companyAppService;
            _companyCashAppService = companyCashAppService;
            _coinAppService = coinAppService;
            _companyCountryAppService = companyCountryAppService;
            _message = "";
            _success = false;
        }

        public ActionResult Index()
        {
            return View();
        
        }

        #region Company

        public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var companies = _companyAppService.GetAll().AsQueryable();

                var totalCount = companies.Count();

                // searching and sorting
                companies = SearchAndSort(requestModel, companies);
                var filteredCount = companies.Count();

                // Paging
                companies = companies.Skip(requestModel.Start).Take(requestModel.Length);

                var dataTablesResponse = new DataTablesResponse(requestModel.Draw, companies.ToList(), filteredCount, totalCount);
                return Json(dataTablesResponse, JsonRequestBehavior.AllowGet);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json(new { Success = false, Message = _message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSpecificCompany(string name, int? id)
        {
            try
            {
                var company = new CompanyForDropdownDto();
                if (id != null)
                    company = _companyAppService.GetForDropdown(name).FirstOrDefault(x => x.Id != id);
                else
                    company = _companyAppService.GetForDropdown(name).FirstOrDefault();

                return Json(new { Success = true, Company = company }, JsonRequestBehavior.AllowGet);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json(new { Success = false, Message = _message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetCompanyWithoutSpecific(int? companyId)
        {
            var companies = _companyAppService.GetAll().Where(x => x.Id != companyId).ToList();
            return Json(companies);
        }

        public ActionResult GetForDropdown()
        {
            var compnies = _companyAppService.GetForDropdown("");
            return Json(compnies);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View("_CreateCompany");
        }

        [HttpPost]
        public ActionResult Create(CompanyInsertDto dto)
        {
            var exist = false;
            if (!ModelState.IsValid)
            {

            }
            if (_companyAppService.CheckIfExist(dto.Name, 0))
            {
                _success = false;
                _message = "توجد عملة بنفس الاسم";
                exist = true;
            }
            else
            {
                var companyDto = _companyAppService.Insert(dto);
                if (companyDto != null)
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
            var dto = _companyAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditCompany", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Edit(CompanyUpdateDto dto)
        {
            var exist = false;

            if (_companyAppService.CheckIfExist(dto.Name, dto.Id.Value))
            {
                _success = false;
                _message = "توجد عملة بنفس الاسم";
                exist = true;
            }
            else
            {
                var companyDto = _companyAppService.Update(dto);
                if (companyDto != null)
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
            var dto = _companyAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeleteCompany", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Delete(CompanyDto dto)
        {
            _companyAppService.Delete(dto.Id.Value);

            return Content("success");
        }

        public ActionResult Detail(int id)
        {
            var dto = _companyAppService.GetById(id);

            var coins = _coinAppService.GetAll();
            ViewBag.Coins = new SelectList(coins, "Id", "Name");

            return View("_DetailsCompany", dto);
        }

        #endregion

        [HttpPost]
        public ActionResult GetCompaniesExceptRelatedId(int id)
        {
            var companies = _companyAppService.GetAll().Where(x => x.Id != id).Select(c => new { Id = c.Id, Name = c.Name });
            return Json(companies);
        }

        public JsonResult CompanyUseCoinInThisCountry(int countryId, int coinId)
        {

            try
            {
                var companies = _companyAppService.GetAll().Where(c => c.HaveCountryById(countryId));
                List<object> company = new List<object>();
                foreach (var item in companies)
                {
                    company.Add(new
                    {
                        id = item.Id,
                        Name = item.Name,
                    });
                }
                if (company.Count == 0)
                    return Json("null");
                return Json(company);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json("null");
            }
        }

        [HttpPost]
        public ActionResult CompaniesDontHaveId(int id)
        {
            var companies = _companyAppService.GetAll().Where(x => x.Id != id).Select(x => new { Id = x.Id, Name = x.Name });
            return Json(companies);
        }

        #region Helper Method

        private IQueryable<CompanyDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<CompanyDto> query)
        {
            // Apply filters
            if (requestModel.Search.Value != string.Empty)
            {
                var value = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Name.Contains(value) 

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