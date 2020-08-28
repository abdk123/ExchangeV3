using BWR.Domain.Model.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Transaction.OuterTransaction
{
    public class OuterTransactionInputDto
    {
        public int? CoinId { get; set; }
        public int? CountryId { get; set; }
        public int? SenderClientId { get; set; }
        public int? ReceiverClientId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public TypeOfPay TypeOfPay { get; set; }
    }
}
