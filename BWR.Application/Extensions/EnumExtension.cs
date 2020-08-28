using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Extensions
{
    public static class EnumExtension
    {
        public static string GetArabicTextValue(this string typeOfPay)
        {
            if (typeOfPay == TypeOfPay.ClientsReceivables.ToString())
            {
                return "ذمم عملاء";
            }
            else if (typeOfPay == TypeOfPay.CompaniesReceivables.ToString())
            {
                return "ذمم شركات";
            }
            else if (typeOfPay == TypeOfPay.Cash.ToString())
            {
                return "نقدي";
            }

            return string.Empty;
        }

        public static string GetArabicTextValue(this TransactionStatus transactionsStatus)
        {
            if (transactionsStatus == TransactionStatus.Notified)
            {
                return "مبلغ";
            }
            else if (transactionsStatus == TransactionStatus.NotNotified)
            {
                return "غير مبلغ";
            }
            else if (transactionsStatus == TransactionStatus.None)
            {
                return "بلا";
            }

            return string.Empty;
        }

        public static string GetArabicTextValue(this TransactionType transactionsType)
        {
            if (transactionsType == TransactionType.ImportTransaction)
            {
                return "حوالات واردة";
            }
            else if (transactionsType == TransactionType.ExportTransaction)
            {
                return "حوالات صادرة";
            }
            

            return string.Empty;
        }
    }
}
