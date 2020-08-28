
using BWR.Application.Common;

namespace BWR.Application.Dtos.Company
{
    public class CompanyCashesDto:EntityDto
    {
        public decimal InitialBalance { get; set; }
        public decimal Total { get; set; }
        public decimal? MaxCreditor { get; set; }
        public decimal? MaxDebit { get; set; }
        public int CoinId { get; set; }
        public string CoinName { get; set; }
        public int CompanyId { get; set; }

        public decimal? ForHim { get; set; }
        public decimal? OnHim { get; set; }
    }
}
