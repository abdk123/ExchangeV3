using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Common;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Treasures;
using BWR.ShareKernel.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Transactions
{
    public class Transaction: Entity
    {
        public Transaction()
        {
            MoenyActions = new List<MoneyAction>();
        }
        
        public string Note { get; set; }
        public string Reason { get; set; }
        public decimal Amount { get; set; }
        public decimal OurComission { get; set; }
        public bool? Deliverd { get; set; }
        public decimal? RecivingAmount { get; set; }
        public DateTime? DeliverdDate { get; set; }
        public virtual TypeOfPay TypeOfPay { get; set; }
        public virtual TransactionStatus TransactionsStatus { get; set; }
        public virtual TransactionType TransactionType { get; set; }

        #region Sender Info
        public int? SenderBranchId { get; set; }
        [ForeignKey("SenderBranchId")]
        public virtual Branch SenderBranch { get; set; }

        public int? SenderClientId { get; set; }
        [ForeignKey("SenderClientId")]
        public virtual Client SenderClient { get; set; }

        public decimal? SenderCleirntCommission { get; set; }

        public int? SenderCompanyId { get; set; }
        [ForeignKey("SenderCompanyId")]
        public virtual Company SenderCompany { get; set; }

        public decimal? SenderCompanyComission { get; set; }
        #endregion

        #region Receiver Info
        public int ReceiverBranchId { get; set; }
        [ForeignKey("ReceiverBranchId")]
        public virtual Branch ReceiverBranch { get; set; }

        public int? ReciverClientId { get; set; }
        [ForeignKey("ReciverClientId")]
        public virtual Client ReciverClient { get; set; }

        public decimal? ReciverClientCommission { get; set; }

        public int? ReceiverCompanyId { get; set; }
        [ForeignKey("ReceiverCompanyId")]
        public virtual Company ReceiverCompany { get; set; }

        public decimal? ReceiverCompanyComission { get; set; }

        #endregion

        public int CoinId { get; set; }
        [ForeignKey("CoinId")]
        public virtual Coin Coin { get; set; }

        public int? CountryId { get; set; }
        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }

        public int TreaseryId { get; set; }
        [ForeignKey("TreaseryId")]
        public virtual Treasury Treasury { get; set; }

        public int? IncomeTransactionCollectionId { get; set; }
        [ForeignKey("IncomeTransactionCollectionId")]
        public virtual IncomeTransactionCollection IncomeTransactionCollection { get; set; }

        public virtual IList<MoneyAction> MoenyActions { get; set; }

        public string FormatingAmount
        {
            get
            {
                string commaNumer = String.Format("{0:N}", Amount);
                var split = commaNumer.Split('.');
                var afterDot = Convert.ToInt32(split[1]);

                return afterDot == 0 ? split[0] : commaNumer;
            }
        }
    }
}
