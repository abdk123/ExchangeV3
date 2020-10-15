using BWR.Application.Dtos.Client;

namespace BWR.Application.Dtos.Company
{
    public class CompanySenderDto
    {
        public int CompanyId { get; set; }
        public decimal CompanyCommission { get; set; }
        public ClientForTransactionDto ReciverClinet { get; set; }
    }
}
