using BWR.Application.Dtos.Company.CompanyCashFlow;
using System.Web.Mvc;
using BWR.Application.Interfaces.Company;

namespace Bwr.WebApp.Controllers
{
    public class CompanyCashFlowController : Controller
    {
        private readonly ICompanyCashFlowAppService _companyCashFlowAppService;

        private string _message;
        private bool _success;

        public CompanyCashFlowController(ICompanyCashFlowAppService companyCashFlowAppService)
        {
            _companyCashFlowAppService = companyCashFlowAppService;
            _message = "";
            _success = false;
        }
        public ActionResult Index(int companyId)
        {
            return View(new CompanyCashFlowInputDto() { CompanyId = companyId });
        }

        // GET: CompanyCashFlow
        public ActionResult Get(CompanyCashFlowInputDto inputDto)
        {
            var companyCashFlows = _companyCashFlowAppService.Get(inputDto);

            return Json(new { data = companyCashFlows }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ConvertMatchingStatus(CompanyMatchDto dto)
        {
            if (_companyCashFlowAppService.ConvertMatchingStatus(dto) != null)
                _success = true;

            return Json(new { Success = _success }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBalanceForCompany(int companyId, int coinId)
        {
            return Json(_companyCashFlowAppService.GetBalanceForCompany(companyId, coinId));
        }
        [HttpPost]
        public void Shaded(int id, bool value)
        {
                _companyCashFlowAppService.Shaded(id, value);
            
        }
    }
}