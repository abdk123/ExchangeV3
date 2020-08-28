using BWR.Application.Common;
using System;

namespace BWR.Application.Dtos.Treasury.TreasuryMoneyAction
{
    public class TreasuryActionsDto: EntityDto
    {
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
        public int CoinId { get; set; }
        public int? MoneyActionId { get; set; }
        public string Date { get; set; }
        public string CreatedBy { get; set; }
        public int? BranchCashFlowId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int? Number { get; set; }
    }
}
