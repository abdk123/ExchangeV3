using System;
using BWR.Application.Common;

namespace BWR.Application.Dtos.Client.ClientCashFlow
{
    public class ClientCashFlowDto : EntityDto
    {
        public decimal Total { get; set; }
        public decimal Amount { get; set; }
        public bool Matched { get; set; }
        public int ClientId { get; set; }
        public int CoinId { get; set; }
        public int MoenyActionId { get; set; }
        public Guid UserId { get; set; }
    }
}