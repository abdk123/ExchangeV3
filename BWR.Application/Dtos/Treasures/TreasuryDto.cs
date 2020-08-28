using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BWR.Domain.Model.Treasures;
namespace BWR.Application.Dtos.Treasury
{
    public class TreasuryDto.cs : EntityDto
    {
         public string Name { get; set; }
         public bool IsEnabled { get; set; }
         public bool IsAvilable { get; set; }
         public int BranchId { get; set; }
         public BranchDto Branch { get; set; }
         public IList<TransactionsDto> Transactions { get; set; }
         public IList<TreasuryCashesDto> TreasuryCashes { get; set; }
         public IList<TreasuryMoneyActionsDto> TreasuryMoneyActions { get; set; }
         public IList<UserTreasueriesDto> UserTreasueries { get; set; }
         public IList<BranchCashFlowsDto> BranchCashFlows { get; set; }
    }
}
