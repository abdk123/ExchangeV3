using BWR.Application.Dtos.Client;
using BWR.Application.Interfaces.Client;
using System.Linq;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers.Client
{
    public class ClientCashController : Controller
    {

        private readonly IClientCashAppService _clientCashAppService;
        private string _message;
        private bool _success;

        public ClientCashController(IClientCashAppService clientCashAppService)
        {
            _clientCashAppService = clientCashAppService;
            _message = "";
            _success = false;
        }

        // GET: ClientCash
        public ActionResult Index(int clientId)
        {
            var dto = new ClientCashDto()
            {
                ClientId = clientId
            };
            return View(dto);
        }

        public ActionResult Get(int clientId)
        {
            var clientCashes = _clientCashAppService.GetClientCashes(clientId).OrderBy(x => x.CoinName).ToList();

            return Json(new { data = clientCashes }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditClientCash(ClientCashesDto dto)
        {
            var clientBalance = _clientCashAppService.UpdateBalance(dto);
            if (clientBalance != null)
                _success = true;
            else
            {
                _success = false;
                _message = "حدثت مشكلة اثناء تعديل البيانات ";
            }

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetClientCashes(int clientId)
        {
            var clientCashes = _clientCashAppService.GetClientCashes(clientId);
            return Json(clientCashes);
        }

        [HttpPost]
        public ActionResult GetClientCashByCoin(int clientId,int coinId)
        {
            var clientCashes = _clientCashAppService.GetClientCashes(clientId).FirstOrDefault(x => x.CoinId == coinId);
            return Json(clientCashes);
        }

        public ActionResult GetClientCashesForOther(int clientId)
        {
            var clientCashes = _clientCashAppService.GetAll().Where(x => x.ClientId != clientId).ToList();

            return Json(clientCashes);
        }

        public ActionResult GetClientMaxAndDeptByCoin(int coinId, int clientId)
        {
            var clientCash = _clientCashAppService.GetClientCashes(clientId).FirstOrDefault(x => x.CoinId == coinId);

            return Json(new
            {
                clientCash.MaxCreditor,
                clientCash.MaxDebit,
                clientCash.Total
            });
        }

        public decimal GetClientBalance(int clientId, int coinId)
        {
            decimal total = 0;
            var clientCash = _clientCashAppService.GetClientCashes(clientId).FirstOrDefault(x => x.CoinId == coinId);
            if (clientCash != null)
            {
                total = clientCash.Total;
            }
            return total;
        }

    }
}