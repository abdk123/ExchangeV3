using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Common
{
    public class BoxAction:  Entity
    {
        public decimal Amount { get; set; }
        
        public bool IsIncmoe { get; set; }
        public string Note { get; set; }

        public int CoinId { get; set; }
        [ForeignKey("CoinId")]
        public virtual Coin Coin { get; set; }
    }
}
