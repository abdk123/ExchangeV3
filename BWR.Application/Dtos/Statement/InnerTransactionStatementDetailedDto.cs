using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Statement
{
    public class InnerTransactionStatementDetailedDto
    {
        public int Id { get; set; }
        public string CoinName { get; set; }
        

        public decimal Amount { get; set; }
        public string TypeOfPay { get; set; }
        public string ReciverName { get; set; }
        public string ReciverAddress { get; set; }
        public string ReciverPhone { get; set; }
        public string State { get; set; }
        public string SenderName { get; set; }
        public string Date { get; set; }
        public string SenderCompany { get; set; }
        public bool IsDiliverd { get; set; }
        public int MoneyActionId { get; set; }


    }
}
