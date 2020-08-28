using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BWR.Domain.Model.Treasures;
namespace BWR.Application.Dtos.TreasuryMoneyAction
{
    public class TreasuryMoneyActionDto.cs : EntityDto
    {
         public Decimal Amount { get; set; }
         public Decimal Total { get; set; }
         public int CoinId { get; set; }
         public CoinDto Coin { get; set; }
         public int TreasuryId { get; set; }
         public TreasuryDto Treasury { get; set; }
         public int? BranchCashFlowId { get; set; }
         public BranchCashFlowDto BranchCashFlow { get; set; }
    }
}
