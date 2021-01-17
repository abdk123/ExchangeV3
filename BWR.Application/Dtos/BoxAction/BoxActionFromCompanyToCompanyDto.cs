using System;
namespace BWR.Application.Dtos.BoxAction
{
    public class BoxActionFromCompanyToCompanyDto
    {
        public int CoinId { get; set; }
        public int FirstCompanyId { get; set; }
        public int SecondCompanyId { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
    }
}
