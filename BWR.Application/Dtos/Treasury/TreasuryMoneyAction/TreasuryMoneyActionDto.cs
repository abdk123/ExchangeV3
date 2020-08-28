using System;
using BWR.Application.Common;
namespace BWR.Application.Dtos.Treasury.TreasuryMoneyAction
{
    public class TreasuryMoneyActionDto : EntityDto
    {
        public Decimal Amount { get; set; }
        public Decimal Total { get; set; }
        public int CoinId { get; set; }
        public int TreasuryId { get; set; }
        public string Created { get; set; }
        public int? BranchCashFlowId { get; set; }
        
    }
}
