
namespace BWR.Application.Dtos.Branch.BranchCommission
{
    public class BranchCommissionInputDto
    {
        public int? CountryId { get; set; }
        public int? CoinId { get; set; }
        public int? BranchId { get; set; }
        public decimal Amount { get; set; }
    }
}
