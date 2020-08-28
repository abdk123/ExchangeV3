using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Client.ClientCashFlow
{
    public class ClientCashFlowInputDto
    {
        public int ClientId { get; set; }
        public int CoinId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
