using BWR.Application.Common;

namespace BWR.Application.Dtos.Company.CompanyCommission
{
    public class CompanyCommissionsDto: EntityDto
    {
        public decimal StartRange { get; set; }
        public decimal EndRange { get; set; }
        public decimal Cost { get; set; }
        public decimal Ratio { get; set; }
        public int CompanyCountryId { get; set; }
        public int CoinId { get; set; }

        public string CoinName { get; set; }
        public string CountryName { get; set; }
    }
}
