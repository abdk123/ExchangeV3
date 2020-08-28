using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Common
{
    public class Exchange: Entity
    {
        public decimal AmountOfFirstCoin { get; set; }
        public decimal AmoutOfSecoundCoin { get; set; }
        
        public decimal? FirstCoinExchangePriceWithMainCoin { get; set; }
        public decimal? FirstCoinSellingPriceWithMainCoin { get; set; }
        public decimal? FirstCoinPurchasingPriceWithMainCoin { get; set; }
        public decimal? SecoundCoinExchangePriceWithMainCoin { get; set; }
        public decimal? SecoundCoinSellingPricWithMainCoin { get; set; }
        public decimal? SecoundCoinPurchasingPriceWithMainCoin { get; set; }
        
        public string Note { get; set; }

        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public int FirstCoinId { get; set; }
        [ForeignKey("FirstCoinId")]
        public virtual Coin FirstCoin { get; set; }

        public int SecoundCoinId { get; set; }
        [ForeignKey("SecoundCoinId")]
        public virtual Coin SecoundCoin { get; set; }

        public int? MainCoinId { get; set; }
        [ForeignKey("MainCoinId")]
        public virtual Coin MainCoin { get; set; }
    }
}
