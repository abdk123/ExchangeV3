using BWR.Application.Common;
using BWR.Application.Dtos.Setting.Coin;

namespace BWR.Application.Dtos.Company.CompanyCommission
{
    public class CompanyCommissionDto: EntityDto
    {
        public decimal StartRange { get; set; }
        public decimal EndRange { get; set; }
        public decimal Cost { get; set; }
        public decimal Ratio { get; set; }
        public bool IsEnabled { get; set; }
        public int CompanyCountryId { get; set; }
        public int CoinId { get; set; }

        public CompanyCountryDto CompanyCountry { get; set; }
        public CoinDto Coin { get; set; }

    }
}
