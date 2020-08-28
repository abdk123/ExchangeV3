
using BWR.Domain.Model.Common;

namespace BWR.Application.Extensions
{
    public static class PublicMoneyExtension
    {
        public static int GetPublicId(this PublicMoney publicMoney)
        {
            if (publicMoney.ExpenseId != null)
                return (int)publicMoney.ExpenseId;
            return (int)publicMoney.IncomeId;
        }
        public static string GetTypeName(this PublicMoney publicMoney)
        {
            if (publicMoney.IncomeId != null)
                return "أيرادات عامة";
            return "مصاريف عامة";
        }
        public static string GetActionName(this PublicMoney publicMoney)
        {
            if (publicMoney.IncomeId != null)
                return publicMoney.PublicIncome.Name;
            return publicMoney.PublicExpense.Name;
        }
    }
}
