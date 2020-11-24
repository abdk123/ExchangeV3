using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Transaction.InnerTransaction
{
    class IncomeTransactionCollectionDto
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string Company { get; set; }
        public string Amount { get; set; }
        public int TransactionCount { get; set; }
    }
}
