using BWR.Application.Dtos.Client;
using BWR.Application.Dtos.Company;
using BWR.Domain.Model.Settings;

namespace BWR.Application.Dtos.Transaction.InnerTransaction
{
    public class InnerTransactionInsertDto
    {
        public int CoinId { get; set; }
        public decimal Amount { get; set; }
        public decimal OurComission { get; set; }
        public ClientForTransactionDto Sender { get; set; }
        public TypeOfPay TypeOfPay { get; set; }
        
        public ClientForTransactionDto ReciverClinet { get; set; }
        public int AgentId { get; set; }
        public decimal AgentCommission { get; set; }
        public CompanyReceiverDto ReciverCompany { get; set; }
    }
}
