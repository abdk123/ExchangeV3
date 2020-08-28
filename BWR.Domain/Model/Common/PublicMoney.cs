using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Common
{
    public class PublicMoney: Entity
    {
        public int? ExpenseId { get; set; }
        [ForeignKey("ExpenseId")]
        public virtual PublicExpense PublicExpense { get; set; }

        public int? IncomeId { get; set; }
        [ForeignKey("IncomeId")]
        public virtual PublicIncome PublicIncome { get; set; }
    }
}