using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Domain.Model.Common
{
    public class Clearing:Entity
    {
        public bool IsIncome { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }

        public int? FromClientId { get; set; }
        [ForeignKey("FromClientId")]
        public Client FromClient { get; set; }

        public int? FromCompanyId { get; set; }
        [ForeignKey("FromCompanyId")]
        public Company FromCompany { get; set; }

        
        public int? ToClientId { get; set; }
        [ForeignKey("ToClientId")]
        public Client ToClient { get; set; }

        public int? ToCompanyId { get; set; }
        [ForeignKey("ToCompanyId")]
        public Company ToCompany { get; set; }

        public int CoinId { get; set; }
        [ForeignKey("CoinId")]
        public Coin Coin { get; set; }
    }
}
