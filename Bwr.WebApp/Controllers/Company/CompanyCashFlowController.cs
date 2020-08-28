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
    }
}