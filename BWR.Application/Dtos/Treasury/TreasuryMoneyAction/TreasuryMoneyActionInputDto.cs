using BWR.Application.Common;
using System;

namespace BWR.Application.Dtos.Treasury.TreasuryMoneyAction
{
    public class TreasuryMoneyActionInputDto:EntityDto
    {
        public int TreasuryId { get; set; }
        public int CoinId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        
    }
}
