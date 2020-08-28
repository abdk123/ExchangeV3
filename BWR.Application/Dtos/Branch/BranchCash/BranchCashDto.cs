using BWR.Application.Common;
using BWR.Application.Dtos.Setting.Coin;

namespace BWR.Application.Dtos.Branch
{
    public class BranchCashDto:EntityDto
    {
        public decimal InitialBalance { get; set; }
        public decimal Total { get; set; }
        public bool IsMainCoin { get; set; }
        public decimal? ExchangePrice { get; set; }
        public decimal? PurchasingPrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public bool IsEnabled { get; set; }
        public int BranchId { get; set; }
        public int CoinId { get; set; }
        public CoinDto Coin { get; set; }

    }
}
