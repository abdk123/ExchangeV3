using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Transactions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Treasures
{
    public class Treasury
    {
        public Treasury()
        {
            Transactions = new List<Transaction>();
            TreasuryCashes = new List<TreasuryCash>();
            TreasuryMoneyActions = new List<TreasuryMoneyAction>();
            UserTreasueries = new List<UserTreasuery>();
            BranchCashFlows = new List<BranchCashFlow>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsAvilable { get; set; }

        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }
        
        public virtual IList<Transaction> Transactions { get; set; }
        public virtual IList<TreasuryCash> TreasuryCashes { get; set; }
        public virtual IList<TreasuryMoneyAction> TreasuryMoneyActions { get; set; }
        public virtual IList<UserTreasuery> UserTreasueries { get; set; }
        public virtual IList<BranchCashFlow> BranchCashFlows { get; set; }
    }
}
