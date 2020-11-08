using BWR.Domain.Model.Common;
using BWR.Domain.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Extensions
{
    public static class BoxActionExensions
    {
        public static string GetActionType(this BoxAction boxAction)
        {
            if (boxAction.BoxActionType == BoxActionType.ExpenseFromTreasury)
                return "مصاريف عامة";
            if (boxAction.BoxActionType == BoxActionType.ExpenseFromTreasuryToClient)
                return "ذمم عملاء";
            if (boxAction.BoxActionType == BoxActionType.ExpenseFromTreasuryToCompany)
                return "ذمم شركات";
            return "GetActionType()";
        }
    }
}
