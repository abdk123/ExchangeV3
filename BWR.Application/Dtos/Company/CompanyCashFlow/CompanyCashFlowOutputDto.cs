using BWR.Application.Common;

namespace BWR.Application.Dtos.Company.CompanyCashFlow
{
    public class CompanyCashFlowOutputDto: EntityDto
    {
        public decimal? Balance { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Commission { get; set; }
        public decimal? SecondCommission { get; set; }
        public string ReceiverName { get; set; }
        public string SenderName { get; set; }
        public string CountryName { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int? Number { get; set; }
        public string Date { get; set; }
        public string Note { get; set; }


    }
}
