namespace BWR.Application.Dtos.BoxAction
{
    public class BoxActionCompanyUpdateDto
    {
        public int MoneyActionId { get; set; }
        public int CoinId { get; set; }
        public int CompanyId { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
    }
}
