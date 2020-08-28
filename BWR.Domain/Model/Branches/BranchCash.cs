using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Branches
{
    public class BranchCash: Entity
    {
        public decimal InitialBalance { get; set; }
        public decimal Total { get; set; }
        public bool IsMainCoin { get; set; }
        public decimal? ExchangePrice { get; set; }
        public decimal? PurchasingPrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public bool IsEnabled { get; set; }

        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public int CoinId { get; set; }
        [ForeignKey("CoinId")]
        public virtual Coin Coin { get; set; }
    }
}