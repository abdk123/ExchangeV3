using BWR.Application.Common;
using BWR.Application.Dtos.BoxAction;
using BWR.Application.Dtos.BranchCashFlow;
using BWR.Application.Dtos.Client.ClientCashFlow;
using BWR.Application.Dtos.Company.CompanyCashFlow;
using BWR.Application.Dtos.Exchange;
using BWR.Application.Dtos.Transaction;
using System.Collections.Generic;

namespace BWR.Application.Dtos.MoneyAction
{
    public class MoneyActionOutputDto:EntityDto
    {
        public int? BoxActionsId { get; set; }
        
        public int? ExchangeId { get; set; }
        
        public int? PubLicMoneyId { get; set; }

        public int? ClearingId { get; set; }
        
        public int? TransactionId { get; set; }
        
        public TransactionDto Transaction { get; set; }
    }
}
