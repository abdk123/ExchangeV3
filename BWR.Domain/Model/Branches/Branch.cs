using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Transactions;
using BWR.Domain.Model.Treasures;
using BWR.ShareKernel.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Branches
{
    public class Branch: Entity
    {
        public Branch()
        {
            BranchCashes = new List<BranchCash>();
            BranchCashFlows = new List<BranchCashFlow>();
            BranchComissions = new List<BranchCommission>();
            Transactions = new List<Transaction>();
            Treasurys = new List<Treasury>();
        }

        public string Name { get; set; }
        public string Address { get; set; }
        public bool IsEnabled { get; set; }

        public int? CountryId { get; set; }
        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }
        
        public virtual IList<BranchCash> BranchCashes { get; set; }
        public virtual IList<BranchCashFlow> BranchCashFlows { get; set; }
        public virtual IList<BranchCommission> BranchComissions { get; set; }
        public virtual IList<Transaction> Transactions { get; set; }
        public virtual IList<Treasury> Treasurys { get; set; }
    }
}
