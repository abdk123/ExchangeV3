using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWR.ShareKernel.Common;

namespace BWR.Application.Dtos.Client
{
    public class ClientCashesDto:Entity
    {
        public decimal InitialBalance { get; set; }
        public decimal Total { get; set; }
        public decimal? MaxCreditor { get; set; }
        public decimal? MaxDebit { get; set; }
        public bool IsEnabled { get; set; }
        public int ClientId { get; set; }
        public int CoinId { get; set; }
        public string CoinName { get; set; }

        public decimal? ForHim { get; set; }
        public decimal? OnHim { get; set; }
    }
}
