using BWR.Application.Common;
using BWR.Application.Dtos.Company.CompanyCommission;
using BWR.Application.Interfaces.Company;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using System.Linq;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers
{
    public class CompanyCommissionController : Controller
    {
        private readonly ICompanyCommissionAppService _companyCommissionAppService;
        private readonly ICompanyCountryAppService _companyCountryAppService;
        private string _message;
        private bool _success;

        public CompanyCommissionController(
            ICompanyCommissionAppService companyCommissionAppService,
            ICompanyCountryAppService companyCountryAppService)
        {
            _companyCommissionAppService = companyCommissionAppService;
            _companyCountryAppService = companyCountryAppService;
            _message = "";
            _success = false;
        }

        public ActionResult Index(int companyId)
        {
            TempData["CompanyId"] = companyId;

            return View(new EntityDto() { Id = companyId });
        }

        // GET: CompanyCommission
        public ActionResult Get(int companyId)
        {
            try
            {
                var companyCommissions = _companyCommissionAppService.GetByCompanyId(companyId).ToList();
                return Json(new { data = companyCommissions }, JsonRequestBehavior.AllowGet);
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
            return View("_CreateCompanyCommission");
        }

        [HttpPost]
        public ActionResult Create(CompanyCommissionInsertDto dto, string commissionType)
        {
            if(_companyCommissionAppService.CheckIfExist(0, dto.CompanyCountryId,dto.CoinId))
            {
                _success = false;
                _message = "لا يمكن تكرار عمولة لنفس البلد ونفس العملة";
            }
            else
            {
                //في حال تغيير النوع يجب تصفير القيمة الاخرى
                if (commissionType == "Cost")
                {
                    dto.Ratio = 0;
                }
                else if (commissionType == "Ratio")
                {
                    dto.Cost = 0;
                }
                var companyCommissionDto = _companyCommissionAppService.Insert(dto);
                if (companyCommissionDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء إضافة البيانات ";
                }
            }

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var dto = _companyCommissionAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditCompanyCommission", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Edit(CompanyCommissionUpdateDto dto, string commissionType)
        {
            if (_companyCommissionAppService.CheckIfExist(dto.Id.Value, dto.CompanyCountryId, dto.CoinId))
            {
                _success = false;
                _message = "لا يمكن تكرار عمولة لنفس البلد ونفس العملة";
            }
            else
            {
                //في حال تغيير النوع يجب تصفير القيمة الاخرى
                if (commissionType == "Cost")
                {
                    dto.Ratio = 0;
                }
                else if (commissionType == "Ratio")
                {
                    dto.Cost = 0;
                }

                var companyCommissionDto = _companyCommissionAppService.Update(dto);
                if (companyCommissionDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء تعديل البيانات ";
                }
            }

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var dto = _companyCommissionAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeleteCompanyCommission", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Delete(CompanyCommissionDto dto)
        {
            _companyCommissionAppService.Delete(dto.Id.Value);

            return Content("success");
        }

        public decimal? CalcComission(int companyId, int countryId, int coinId, decimal amount)
        {
            var companyCountry = _companyCountryAppService.GetCountriesForCompany(companyId).FirstOrDefault(x => x.CountryId == countryId);
            if (companyCountry == null)
            {
                return 0;
            }
            var companyCountryId = companyCountry.Id;
            var companyComission=_companyCommissionAppService.GetByCompanyId(companyId)
                .Where(c => c.CompanyCountryId == companyCountryId && c.CoinId == coinId && c.StartRange <= amount && (c.EndRange == null || amount <= c.EndRange)).OrderByDescending(c => c.Id).FirstOrDefault();
            
            if (companyComission == null)
                return 0;
            if (companyComission.Cost != 0)
                return companyComission.Cost;
            return (amount * companyComission.Ratio) / 100;
        }
    }
}