using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Companies
{
    public class CompanyCommission: Entity
    {
        public decimal StartRange { get; set; }
        public decimal? EndRange { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Ratio { get; set; }
        public bool IsEnabled { get; set; }

        public int CoinId { get; set; }
        [ForeignKey("CoinId")]
        public virtual Coin Coin { get; set; }

        public int CompanyCountryId { get; set; }
        [ForeignKey("CompanyCountryId")]
        public virtual CompanyCountry CompanyCountry { get; set; }
    }
}