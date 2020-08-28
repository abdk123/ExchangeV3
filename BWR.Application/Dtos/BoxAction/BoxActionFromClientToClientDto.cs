
namespace BWR.Application.Dtos.BoxAction
{
    public class BoxActionFromClientToClientDto
    {
        public int CoinId { get; set; }
        public int FirstClientId { get; set; }
        public int SecondClientId { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
    }
}
