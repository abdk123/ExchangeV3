using BWR.Application.Common;
using System;

namespace BWR.Application.Dtos.Common
{
    public class ExchangeInputDto: EntityDto
    {
        public DateTime Date { get; set; }
        public int FirstCoinId { get; set; }
        public int SecondCoinId { get; set; }
        public int? AgentId { get; set; }
        public int? CompanyId { get; set; }
        public int TypeOfPay { get; set; }
        public int ActionType { get; set; }
        public decimal AmountOfFirstCoin { get; set; }
        public decimal AmoutOfSecondCoin { get; set; }
        public string Note { get; set; }
    }
}
