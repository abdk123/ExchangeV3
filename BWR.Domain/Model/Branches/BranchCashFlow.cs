using BWR.Domain.Model.Common;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Treasures;
using BWR.ShareKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Branches
{
    public class BranchCashFlow: Entity
    {
        public decimal Total { get; set; }
        public decimal Amount { get; set; }

        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public int CoinId { get; set; }
        [ForeignKey("CoinId")]
        public virtual Coin Coin { get; set; }

        public int MonyActionId { get; set; }
        [ForeignKey("MonyActionId")]
        public virtual MoneyAction MoenyAction { get; set; }

        public int TreasuryId { get; set; }
        [ForeignKey("TreasuryId")]
        public virtual Treasury Treasury { get; set; }
    }
}