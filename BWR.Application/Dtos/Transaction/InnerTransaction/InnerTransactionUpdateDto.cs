using BWR.Application.Common;
using BWR.Application.Dtos.Client;
using BWR.Application.Dtos.Company;
using BWR.Domain.Model.Settings;
using System;

namespace BWR.Application.Dtos.Transaction.InnerTransaction
{
    public class InnerTransactionUpdateDto: EntityDto
    {
        public int MainCompanyId { get; set; }
        public string Note { get; set; }
        public int CoinId { get; set; }
        public decimal Amount { get; set; }
        public decimal OurComission { get; set; }
        public ClientForTransactionDto SenderClient { get; set; }
        public TypeOfPay TypeOfPay { get; set; }

        public ClientForTransactionDto ReceiverClient { get; set; }
        public int AgentId { get; set; }
        public decimal AgentCommission { get; set; }
        public CompanySenderDto SenderCompany { get; set; }
        public DateTime Date { get; set; }
    }
}
