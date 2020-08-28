using BWR.Application.Dtos.Setting.Coin;

namespace BWR.Application.Dtos.Treasury
{
    public class TreasuryCashDto
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public decimal Amount { get; set; }
        public bool IsEnabled { get; set; }

        public int CoinId { get; set; }
        public CoinDto Coin { get; set; }
        public int TreasuryId { get; set; }
    }
}
