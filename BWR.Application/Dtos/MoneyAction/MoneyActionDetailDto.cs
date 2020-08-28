using BWR.Application.Common;
using BWR.Application.Dtos.Client.ClientCashFlow;
using BWR.Application.Dtos.Company.CompanyCashFlow;
using System.Collections.Generic;

namespace BWR.Application.Dtos.MoneyAction
{
    public class MoneyActionDetailDto: EntityDto
    {
        public MoneyActionDetailDto()
        {
            CompanyCashFlows = new List<CompanyCashFlowDto>();
            ClientCashFlows = new List<ClientCashFlowDto>();
        }
        public int? ExpenseId { get; set; }
        public int? IncomeId { get; set; }

        public virtual IList<CompanyCashFlowDto> CompanyCashFlows { get; set; }
        public virtual IList<ClientCashFlowDto> ClientCashFlows { get; set; }
    }
}
