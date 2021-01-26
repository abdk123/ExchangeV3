using BWR.Domain.Model.Common;
using BWR.Domain.Model.Security;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Transactions;
using BWR.ShareKernel.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Clients
{
    public class ClientCashFlow : Entity
    {
        public decimal Amount { get; set; }
        public bool Matched { get; set; }

        public int ClientId { get; set; }
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        public int CoinId { get; set; }
        [ForeignKey("CoinId")]
        public virtual Coin Coin { get; set; }

        public int MoenyActionId { get; set; }
        [ForeignKey("MoenyActionId")]
        public virtual MoneyAction MoenyAction { get; set; }

        public Guid? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public bool? Shaded { get; set; }
        public decimal RealAmount
        {
            get
            {
                if (this.MoenyAction.TransactionId != null&&this.MoenyAction.BoxActionsId==null)
                {
                    var tranasaction = this.MoenyAction.Transaction;
                    if(tranasaction.TransactionType== TransactionType.ExportTransaction)
                    {
                        return this.Amount + (tranasaction.SenderCleirntCommission ?? 0) - tranasaction.OurComission;
                    }
                    else
                    {
                        return this.Amount + (tranasaction.ReciverClientCommission ?? 0);
                    }
                }
                return this.Amount;
            }
        }
    }
}
