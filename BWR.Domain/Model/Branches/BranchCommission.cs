using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Branches
{
    public class BranchCommission: Entity
    {
        public decimal StartRange { get; set; }
        public decimal EndRange { get; set; }
        public decimal Cost { get; set; }
        public decimal Ratio { get; set; }
        public bool IsEnabled { get; set; }

        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public int CoinId { get; set; }
        [ForeignKey("CoinId")]
        public virtual Coin Coin { get; set; }

        public int CountryId { get; set; }
        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }
    }
}