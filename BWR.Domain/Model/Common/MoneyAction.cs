using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Transactions;
using BWR.ShareKernel.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Common
{
    public class MoneyAction: Entity
    {
        public MoneyAction()
        {
            CompanyCashFlows = new List<CompanyCashFlow>();
            ClientCashFlows = new List<ClientCashFlow>();
            BranchCashFlows = new List<BranchCashFlow>();
        }

        public int? BoxActionsId { get; set; }
        [ForeignKey("BoxActionsId")]
        public virtual BoxAction BoxAction { get; set; }

        public int? ExchangeId { get; set; }
        [ForeignKey("ExchangeId")]
        public virtual Exchange Exchange { get; set; }

        public int? PubLicMoneyId { get; set; }
        [ForeignKey("PubLicMoneyId")]
        public virtual PublicMoney PublicMoney { get; set; }

        public int? ClearingId { get; set; }
        [ForeignKey("ClearingId")]
        public virtual Clearing Clearing { get; set; }

        public int? TransactionId { get; set; }
        [ForeignKey("TransactionId")]
        public virtual Transaction Transaction { get; set; }

        public virtual IList<CompanyCashFlow> CompanyCashFlows { get; set; }
        public virtual IList<ClientCashFlow> ClientCashFlows { get; set; }
        public virtual IList<BranchCashFlow> BranchCashFlows { get; set; }
    }
}
