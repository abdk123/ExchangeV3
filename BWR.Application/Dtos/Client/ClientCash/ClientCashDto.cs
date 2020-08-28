using BWR.Application.Common;

namespace BWR.Application.Dtos.Client
{
    public class ClientCashDto : EntityDto
    {
        public decimal InitialBalance { get; set; }
        public decimal Total { get; set; }
        public decimal? MaxCreditor { get; set; }
        public decimal? MaxDebit { get; set; }
        public bool IsEnabled { get; set; }
        public int ClientId { get; set; }
        public int CoinId { get; set; }
        
    }
}