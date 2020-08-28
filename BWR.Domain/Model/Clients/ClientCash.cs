using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Clients
{
    public class ClientCash: Entity
    {
        public decimal InitialBalance { get; set; }
        public decimal Total { get; set; }
        public decimal? MaxCreditor { get; set; }
        public decimal? MaxDebit { get; set; }
        public bool IsEnabled { get; set; }

        public int ClientId { get; set; }
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        public int CoinId { get; set; }
        [ForeignKey("CoinId")]
        public virtual Coin Coin { get; set; }
    }
}
