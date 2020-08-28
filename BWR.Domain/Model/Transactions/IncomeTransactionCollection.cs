using BWR.Domain.Model.Companies;
using BWR.ShareKernel.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Transactions
{
    public class IncomeTransactionCollection:Entity
    {
        public IncomeTransactionCollection()
        {
            Transactions = new List<Transaction>();
        }
        
        public string Note { get; set; }

        public int CompnayId { get; set; }
        [ForeignKey("CompnayId")]
        public virtual Company Company { get; set; }
        
        public virtual IList<Transaction> Transactions { get; set; }
    }
}
