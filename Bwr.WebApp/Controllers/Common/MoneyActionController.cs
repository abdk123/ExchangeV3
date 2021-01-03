using BWR.Application.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BWR.Domain.Model.Transactions;

namespace Bwr.WebApp.Controllers.Common
{
    public class MoneyActionController : Controller
    {
        private readonly IMoneyActionAppService _moneyActionAppService;

        public MoneyActionController(IMoneyActionAppService moneyActionAppService)
        {
            _moneyActionAppService = moneyActionAppService;
        }

        [HttpGet]
        public ActionResult Index(int id)
        {
            var moenyAction = _moneyActionAppService.GetById(id);
            if (moenyAction == null)
            {
                return HttpNotFound();
            }
            if (moenyAction.TransactionId != null)
            {
                if (moenyAction.Transaction.TransactionType== TransactionType.ExportTransaction)
                {
                    return RedirectToAction("OuterTransactionDetails", "OuterTransaction", new { transactionId = moenyAction.TransactionId });
                }
                else
                {
                    //if (moenyAction.Transaction.Deliverd == true)
                    //{
                        return RedirectToAction("InnerTransactionDetails", "InnerTransaction", new { transactionId = moenyAction.TransactionId });
                    //}
                }
            }
            else if (moenyAction.BoxActionsId != null || moenyAction.ClearingId != null)
            {
                return RedirectToAction("BoxActionDetails", "BoxAction", new { moneyActionId = moenyAction.Id });
            }
            
            return View();
        }
        [HttpPost]
        public ActionResult DeleteMoenyAction(int id)
        {
             
            return Json(_moneyActionAppService.Delete(id));
        }
    }
}