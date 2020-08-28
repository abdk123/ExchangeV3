using BWR.Application.Dtos.Company;
using BWR.Application.Interfaces.Company;
using System.Linq;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers.Company
{
    public class CompanyCashController : Controller
    {
        
        private readonly ICompanyCashAppService _companyCashAppService;
        private string _message;
        private bool _success;

        public CompanyCashController(ICompanyCashAppService companyCashAppService)
        {
            _companyCashAppService = companyCashAppService;
            _message = "";
            _success = false;
        }

        // GET: CompanyCash
        public ActionResult Index(int companyId)
        {
            var dto = new CompanyCashDto()
            {
                CompanyId = companyId
            };
            return View(dto);
        }

        public ActionResult Get(int companyId)
        {
            var companyCashs = _companyCashAppService.GetCompanyCashes(companyId).OrderBy(x => x.CoinName).ToList();

            return Json(new { data = companyCashs }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditCompanyCash(CompanyCashesDto dto)
        {
            var companyBalance = _companyCashAppService.UpdateBalance(dto);
            if (companyBalance != null)
                _success = true;
            else
            {
                _success = false;
                _message = "حدثت مشكلة اثناء تعديل البيانات ";
            }

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        public decimal GetCompanyBalance(int companyId, int coinId)
        {
            return _companyCashAppService.GetCompanyCashes(companyId).FirstOrDefault(x => x.CoinId == coinId).Total;
        }

        public ActionResult GetCompanyCashes(int companyId)
        {
            var companyCashes = _companyCashAppService.GetCompanyCashes(companyId);
            return Json(companyCashes);
        }

        public JsonResult GetCompanyCashesForOther(int companyId)
        {
            var companyCashes = _companyCashAppService.GetAll().Where(x => x.CompanyId != companyId).ToList();

            return Json(companyCashes);
        }

        [HttpPost]
        public ActionResult GetCompanyMaxAndDeptByCoin(int coinId, int companyId)
        {
            var companyCash = _companyCashAppService.GetCompanyCashes(companyId).FirstOrDefault(x => x.CoinId == coinId);

            return Json(new
            {
                companyCash.MaxCreditor,
                companyCash.MaxDebit,
                companyCash.Total
            });
        }
        
    }
}