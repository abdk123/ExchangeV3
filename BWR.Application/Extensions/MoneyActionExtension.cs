using BWR.Domain.Model.Common;
using BWR.Domain.Model.Enums;
using BWR.Domain.Model.Settings;
using BWR.Infrastructure.Common;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BWR.Application.Extensions
{
    
    public static class MoneyActionExtension
    {
        public static decimal? OurCommission(this MoneyAction moneyAction)
        {
            if (moneyAction.BoxActionsId != null)
            {
                return null;
            }
            if (moneyAction.Transaction != null)
            {
                return moneyAction.Transaction.OurComission;
            }
            return null;
        }

        public static int? GetActionId(this MoneyAction moneyAction)
        {
            if (moneyAction.TransactionId != null)
                return (int)moneyAction.TransactionId;
            //if (moneyAction.PublicMoney != null)
            //    return null;
            //return -1;
            if (moneyAction.BoxAction != null)
                return moneyAction.BoxAction.Id;
            if (moneyAction.Exchange != null)
                return moneyAction.Exchange.Id;
            if (moneyAction.ClearingId != null)
                return moneyAction.ClearingId;
            return -1;
        }

        public static string GetNote(this MoneyAction moneyAction, Requester requester, int? objectId)
        {
            if (moneyAction.TransactionId != null)
                return moneyAction.Transaction.Note;
            //if (moneyAction.PublicMoney != null&&moneyAction.BoxAction!=null)
            //    return "";
            //return "none";
            if (moneyAction.BoxAction != null)
                if (moneyAction.PubLicMoneyId == null)
                    return moneyAction.BoxAction.Note;
                else
                    return moneyAction.BoxAction.Note + "/" + moneyAction.PublicMoney.GetActionName();
            if (moneyAction.ClearingId != null)
            {
                return moneyAction.Clearing.GetNote(requester, (int)objectId);
            }
            return "GetNoteMoenyAction";
        }

        public static decimal Comission(this MoneyAction moneyAction, int companyId)
        {
            if (moneyAction.Transaction != null)
            {
                return moneyAction.Transaction.CompanyComission(companyId);
            }
            return 0;
        }

        public static decimal SecounCompanyCommission(this MoneyAction moneyAction, int companyId)
        {
            if (moneyAction.Transaction != null)
            {
                return moneyAction.Transaction.SecounCompanyCommission(companyId);
            }
            return 0;
        }

        public static decimal ClientComission(this MoneyAction moneyAction, int clientId)
        {
            if (moneyAction.BoxActionsId != null)
            {
                return 0;
            }
            if (moneyAction.Transaction != null)
            {
                return moneyAction.Transaction.ClientCommission(clientId: clientId);
            }
            return 0;
        }

        public static string ReciverName(this MoneyAction moneyAction, Requester requester, int requeserId)
        {
            if (moneyAction.TransactionId != null)
                return moneyAction.Transaction.ReceiverName();
            if (moneyAction.Clearing != null)
            {
                return moneyAction.Clearing.ReciverName(requester, requeserId);
            }
            return "";
        }

        public static string SenderName(this MoneyAction moneyAction, Requester requester, int requeserId)
        {
            if (moneyAction.TransactionId != null)
                return moneyAction.Transaction.SenderName();
            if (moneyAction.Clearing != null)
            {
                return moneyAction.Clearing.GetSenderName(requester, (int)requeserId);
            }
            if (moneyAction.PubLicMoneyId != null)
            {
                return moneyAction.PublicMoney.GetActionName();
            }
            return "";
        }

        public static string CountryName(this MoneyAction moneyAction)
        {
            if (moneyAction.Transaction != null)
                if (moneyAction.Transaction.Country != null)
                    return moneyAction.Transaction.Country.Name;
            return "";
        }

        public static string GetDate(this MoneyAction moneyAction)
        {
            if (moneyAction.Transaction != null)
            {
                if (moneyAction.Transaction.Deliverd != null)
                {
                    if ((bool)moneyAction.Transaction.Deliverd)
                    {
                        return ((DateTime)moneyAction.Transaction.DeliverdDate).ToString("dd/MM/yyyy", new CultureInfo("ar-AE"));
                    }
                }
            }
            return moneyAction.Created.Value.ToString("dd/MM/yyyy", new CultureInfo("ar-AE"));
        }

        public static string CreateBy(this MoneyAction moneyAction)
        {
            if (moneyAction.Transaction != null)
            {
                return moneyAction.Transaction.CreateBy();
            }
            return moneyAction.CreatedBy;
        }

        public static string GetTypeName(this MoneyAction moneyAction, Requester requester, int? objectId)
        {
            if (moneyAction.BoxAction != null)
            {
                if (requester == Requester.Branch && moneyAction.PubLicMoneyId != null)
                {
                    if (moneyAction.BoxAction.IsIncmoe)
                        return "قبض";
                    return "صرف";
                }
                if (moneyAction.Clearing == null && moneyAction.PubLicMoneyId == null && requester != Requester.Branch)
                {
                    if (moneyAction.BoxAction.IsIncmoe)
                        return "قبض";
                    return "صرف";
                }
                else if (moneyAction.ClearingId != null)
                {
                    return moneyAction.Clearing.GetTypeName(requester, (int)objectId);
                }
                else if (moneyAction.PubLicMoneyId != null)
                {
                    return moneyAction.BoxAction.IsIncmoe ? "صرف له" : "قبض منه";
                }
                return moneyAction.BoxAction.IsIncmoe ? "قبض" : "صرف";
            }
            if (moneyAction.TransactionId != null)
                return moneyAction.Transaction.GetTypeName(requester, objectId);
            if (moneyAction.PublicMoney != null)
                return moneyAction.PublicMoney.GetTypeName();
            //get Company or ClientName 

            if (moneyAction.ExchangeId != null)
            {
                var unitOfWork = new UnitOfWork<MainContext>();
                var repository = new GenericRepository<Coin>(unitOfWork);
                return repository.GetById(moneyAction.Exchange.FirstCoinId).Name;
            }
            if (moneyAction.ClearingId != null)
            {
                return moneyAction.Clearing.GetTypeName(requester, (int)objectId);
            }
            return "GetTypeName";
        }
        


    }
}
