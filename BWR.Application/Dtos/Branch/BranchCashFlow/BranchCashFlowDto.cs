using System;
using BWR.Application.Common;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Common;
using BWR.Application.Dtos.Setting.Coin;
using BWR.Application.Dtos.Treasury;

namespace BWR.Application.Dtos.BranchCashFlow
{
    public class BranchCashFlowDto : EntityDto
    {
         public Decimal Total { get; set; }
         public Decimal Amount { get; set; }
         public int BranchId { get; set; }
         public BranchDto Branch { get; set; }
         public int CoinId { get; set; }
         public CoinDto Coin { get; set; }
         public int MonyActionId { get; set; }
         public MoneyActionDto MoneyAction { get; set; }
         public int TreasuryId { get; set; }
         public TreasuryDto Treasury { get; set; }
    }
}
