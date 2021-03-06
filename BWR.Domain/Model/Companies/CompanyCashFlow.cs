﻿using BWR.Domain.Model.Common;
using BWR.Domain.Model.Security;
using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Companies
{
    public class CompanyCashFlow : Entity
    {
        public decimal Amount { get; set; }
        public bool Matched { get; set; }
        public bool? Shaded { get; set; }
        public int CoinId { get; set; }
        [ForeignKey("CoinId")]
        public virtual Coin Coin { get; set; }

        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        public int? CompanyUserMatched { get; set; }
        [ForeignKey("CompanyUserMatched")]
        public virtual CompanyUser CompanyUser { get; set; }

        public int MoneyActionId { get; set; }
        [ForeignKey("MoneyActionId")]
        public virtual MoneyAction MoenyAction { get; set; }

        public Guid? UserMatched { get; set; }
        [ForeignKey("UserMatched")]
        public virtual User User { get; set; }
        public decimal RealAmount
        {
            get
            {
                if (this.MoenyAction.Transaction != null)
                {
                    var transaction = this.MoenyAction.Transaction;
                    if (transaction.TransactionType == Transactions.TransactionType.ExportTransaction)
                    {
                        if (transaction.SenderCompanyId == this.CompanyId)
                            return this.Amount + (transaction.SenderCompanyComission ?? 0);
                        if (transaction.ReceiverCompanyId == this.CompanyId)
                            return this.Amount + (transaction.ReceiverCompanyComission ?? 0)-transaction.OurComission;
                    }
                    else
                    {
                        if (transaction.ReceiverCompanyId == this.CompanyId)
                            return this.Amount - (transaction.OurComission);
                    }
                }
                return this.Amount;
            }
        }
    }
}