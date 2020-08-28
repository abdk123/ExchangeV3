using BWR.Application.Dtos.Company;
using BWR.Application.Interfaces.Company;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers
{
    public class CompanyCountryController : Controller
    {
        private readonly ICompanyCountryAppService _companyCountryAppService;
        private string _message;
        private bool _success;

        public CompanyCountryController(ICompanyCountryAppService companyCountryAppService)
        {
            _companyCountryAppService = companyCountryAppService;
            _message = "";
            _success = false;
        }

        // GET: CompnyCountry
        public ActionResult Index(int companyId)
        {
            TempData["CompanyId"] = companyId;
            var dto = new CompanyCountryDto()
            {
                CompanyId = companyId
            };
            return View(dto);
        }

       
        public ActionResult Get(int companyId)
        {
            try
            {
                var companyCountries = _companyCountryAppService.GetCountriesForCompany(companyId).ToList();

                return Json(new { data = companyCountries }, JsonRequestBehavior.AllowGet);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json(new { Success = false, Message = _message }, JsonRequestBehavior.AllowGet);
            }
            
        }

        public ActionResult GetForDropdown(int companyId)
        {
            var countries = _companyCountryAppService.GetCompanyCountriesForDropdown(companyId);
            return Json(new { Countries = countries }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddCompanyCountry(CompanyCountryDto dto)
        {
            var exist = false;
            if (_companyCountryAppService.CheckIfExist(dto.CompanyId, dto.CountryId))
            {
                _success = false;
                _message = "توجد منطقة بنفس الاسم";
                exist = true;
            }
            else
            {
                var companyCountryDto = _companyCountryAppService.Insert(dto);
                if (companyCountryDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء الإضافة";
                }
            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteCompanyCountry(int id)
        {
            _companyCountryAppService.Delete(id);
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}