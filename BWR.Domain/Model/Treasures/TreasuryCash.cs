using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Treasures
{
    public class TreasuryCash: Entity
    {
        public decimal Total { get; set; }
        public bool IsEnabled { get; set; }

        public int CoinId { get; set; }
        [ForeignKey("CoinId")]
        public virtual Coin Coin { get; set; }

        public int TreasuryId { get; set; }
        [ForeignKey("TreasuryId")]
        public virtual Treasury Treasury { get; set; }
    }
}