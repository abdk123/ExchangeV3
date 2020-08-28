
using BWR.Application.Common;

namespace BWR.Application.Dtos.Company
{
    public class CompanyCashDto : EntityDto
    {
        public decimal InitialBalance { get; set; }
        public decimal Total { get; set; }
        public int CoinId { get; set; }
        public int CompanyId { get; set; }
        
    }
}
