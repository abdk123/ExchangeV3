using BWR.Application.Dtos.Transaction;
using BWR.Domain.Model.Common;
using BWR.Domain.Model.Enums;
using BWR.Domain.Model.Transactions;
using System.Collections.Generic;

namespace BWR.Application.Extensions
{
    public static class TransactionExtension
    {
        public static string GetTypeName(this Transaction transaction, Requester requester, int? objectId)
        {
            if (requester == Requester.Company || requester == Requester.Agent && objectId != null)
            {
                if (transaction.ReceiverCompanyId == (int)objectId)
                {
                    return "حوالة منه";
                }
                return "حوالة له";
            }
            if (requester == Requester.Agent && objectId != null)
            {
                if (transaction.ReciverClientId == (int)objectId)
                {
                    return "حوالة له";
                }
            }
            if (transaction.IsOuterTransaction())
                return "قبض";
            return "حوالة مباشرة";
        }

        public static string CreateBy(this Transaction transaction)
        {
            if (transaction.IsOuterTransaction())
                return transaction.CreatedBy;
            return transaction.ModifiedBy;
        }

        public static string GetActionName(this Transaction transaction)
        {
            if (transaction.IsOuterTransaction())
                return "حوالة ندقية";
            return transaction.ReciverClient != null ? transaction.ReciverClient.FullName : "";

        }

        public static decimal CompanyComission(this Transaction transaction, int companyId)
        {
            if (transaction.SenderCompanyId == companyId)
                if (transaction.SenderCompanyComission == null)
                    return 0;
                else
                    return (decimal)transaction.SenderCompanyComission;
            else if (transaction.ReceiverCompanyId == companyId)
            {
                return transaction.OurComission;
            }
            return 0;
        }

        public static decimal SecounCompanyCommission(this Transaction transaction, int companyId)
        {
            if (transaction.ReceiverCompanyId == companyId)
                if (transaction.ReceiverCompanyComission == null)
                    return 0;
                else
                    return (decimal)transaction.ReceiverCompanyComission;
            return 0;
        }

        public static decimal ClientCommission(this Transaction transaction, int clientId)
        {
            if (transaction.SenderClientId == clientId)
            {
                if (transaction.SenderCleirntCommission != null)
                    return (decimal)transaction.SenderCleirntCommission;
            }
            else if (transaction.ReciverClientId == clientId)
            {
                if (transaction.ReciverClientCommission != null)
                    return (decimal)transaction.ReciverClientCommission;
            }
            return 0;
        }

        public static bool IsOuterTransaction(this Transaction transaction)
        {
            return transaction.TransactionType == TransactionType.ExportTransaction;

        }

        public static string ReceiverName(this Transaction transaction)
        {
            if (transaction.ReciverClient == null)
                return "";
            return transaction.ReciverClient.FullName;
        }

        public static string SenderName(this Transaction transaction)
        {
            if (transaction.SenderClient == null)
                return "";
            return transaction.SenderClient.FullName;
        }

        public static int MoneyActionId(this Transaction transaction)
        {
            return transaction.MoenyActions[0].Id;
        }
    }
}
