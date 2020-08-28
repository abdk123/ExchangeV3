using BWR.Application.Common;

namespace BWR.Application.Dtos.Branch.BranchCashFlow
{
    public class BranchCashFlowOutputDto:EntityDto
    {
        public decimal? Balance { get; set; }
        public decimal? Amount { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Date { get; set; }
        public string Note { get; set; }
        public string CreatedBy { get; set; }
        public int? MoneyActionId { get; set; }
    }
}
