using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Treasures
{
    public class TreasuryMoneyAction: Entity
    {
        public decimal Amount { get; set; }
        public decimal Total { get; set; }

        public int CoinId { get; set; }
        [ForeignKey("CoinId")]
        public virtual Coin Coin { get; set; }

        public int TreasuryId { get; set; }
        [ForeignKey("TreasuryId")]
        public virtual Treasury Treasury { get; set; }

        public int? BranchCashFlowId { get; set; }
        [ForeignKey("BranchCashFlowId")]
        public virtual BranchCashFlow BranchCashFlow { get; set; }
    }
}