
using BWR.Domain.Model.Enums;

namespace BWR.Application.Dtos.BoxAction
{
    public class BoxActionUpdateDto
    {
        public int MoneyActionId { get; set; }
        public int CoinId { get; set; }
        public int? FirstCompanyId { get; set; }
        public int? SecondCompanyId { get; set; }
        public int? FirstClientId { get; set; }
        public int? SecondClientId { get; set; }
        public int? ExpensiveId { get; set; }
        public int? IncomeId { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public string BoxActionType { get; set; }
        public bool IsIncome { get; set; }
        public decimal FirstBalanceFeforeAction { get; set; }
        public decimal SecondBalanceFeforeAction { get; set; }
    }
}
