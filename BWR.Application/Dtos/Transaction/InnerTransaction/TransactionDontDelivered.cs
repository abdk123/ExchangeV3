using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Transaction.InnerTransaction
{
    class TransactionDontDelivered
    {
        public int Id { get; set; }
        public int MoneyActionId { get; set; }
        public decimal Amount { get; set; }
        public string Coin { get; set; }
        public string SenderName { get; set; }
        public string ReciverName { get; set; }
        public string Address { get; set; }
        public string TransactionStatus { get; set; }
        public string Company { get; set; }
        public string Note { get; set; }
        public string Date { get; set; }
        
    }
}
