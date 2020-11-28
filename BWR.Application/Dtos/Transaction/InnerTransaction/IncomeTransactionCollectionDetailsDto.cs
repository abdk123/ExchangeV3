using BWR.Domain.Model.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Transaction.InnerTransaction
{
    public class IncomeTransactionCollectionDetailsDto
    {
        public IncomeTransactionCollectionDetailsDto()
        {
            IncomeTransactionDetails = new List<IncomeTransactionDetailsDto>();
        }
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CompanyId { get; set; }
        public string Note { get; set; }
        public List<IncomeTransactionDetailsDto> IncomeTransactionDetails { get; set; }
    }
    public class IncomeTransactionDetailsDto
    {
        public int CoinId { get; set; }
        public decimal Amount { get; set; }
        public decimal OurComission { get; set; }
        public TypeOfPay TypeOfPay { get; set; }
        public int? SenderClientId { get; set; }
        public int? ReciverClientId { get; set; }
        public decimal? ReciverClientCommission { get; set; }
        public int? ReceiverCompanyId { get; set; }
        public decimal? ReceiverCompanyComission { get; set; }
        public decimal? SenderCompanyComission { get; set; }
        public int? SenderCompanyId { get; set; }
    }

}
