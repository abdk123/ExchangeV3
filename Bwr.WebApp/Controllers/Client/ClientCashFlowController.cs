

using System.Web.Mvc;
using BWR.Application.Dtos.Client.ClientCashFlow;
using BWR.Application.Interfaces.Client;

namespace Bwr.WebApp.Controllers
{
    public class ClientCashFlowController : Controller
    {
        private readonly IClientCashFlowAppService _clientCashFlowAppService;

        private string _message;
        private bool _success;

        public ClientCashFlowController(IClientCashFlowAppService clientCashFlowAppService)
        {
            _clientCashFlowAppService = clientCashFlowAppService;
            _message = "";
            _success = false;
        }
        public ActionResult Index(int clientId)
        {
            return View(new ClientCashFlowInputDto() { ClientId = clientId });
        }

        // GET: ClientCashFlow
        public ActionResult Get(ClientCashFlowInputDto inputDto)
        {
            var clientCashFlows = _clientCashFlowAppService.Get(inputDto);

            return Json(new { data = clientCashFlows }, JsonRequestBehavior.AllowGet);
        }
    }
}