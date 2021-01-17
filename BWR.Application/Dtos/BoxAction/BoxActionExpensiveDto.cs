using System;
namespace BWR.Application.Dtos.BoxAction
{
    public class BoxActionExpensiveDto
    {
        public int CoinId { get; set; }
        public int ExpensiveId { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
    }
}
