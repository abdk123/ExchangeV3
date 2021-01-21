using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Transaction.OuterTransaction
{
    public class OuterTransactionEditInitialDto: OuterTransactionInsertInitialDto
    {
        public int SenderCompanyId { get; set; }
        public decimal SenderCompanyBalanceBeforeAction { get; set; }
        public int? SenderClientId { get; set; }
        public decimal? SednderClientBalanceBeforeAction { get; set; }
    }
}
