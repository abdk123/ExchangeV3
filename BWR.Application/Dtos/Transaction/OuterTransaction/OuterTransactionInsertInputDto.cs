using BWR.Domain.Model.Settings;

namespace BWR.Application.Dtos.Transaction.OuterTransaction
{
    public class OuterTransactionInsertInputDto
    {
        public int? TreasuryId { get; set; }
        public int? CoinId { get; set; }
        public int? CountryId { get; set; }
        public TypeOfPay TypeOfPay { get; set; }
        public int? AgentId { get; set; }
        public int? ClientId { get; set; }
    }
}
