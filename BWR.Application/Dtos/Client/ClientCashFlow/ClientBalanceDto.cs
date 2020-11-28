
namespace BWR.Application.Dtos.Client.ClientCashFlow
{
    public class ClientBalanceDto
    {
        public int CoinId { get; set; }
        public bool IsMainCoin { get; set; }
        public decimal Balance { get; set; }
    }
}
