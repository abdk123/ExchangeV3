using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BWR.Domain.Model.Treasures;
namespace BWR.Application.Dtos.TreasuryCash
{
    public class TreasuryCashDto.cs : EntityDto
    {
         public Decimal Total { get; set; }
         public bool IsEnabled { get; set; }
         public int CoinId { get; set; }
         public CoinDto Coin { get; set; }
         public int TreasuryId { get; set; }
         public TreasuryDto Treasury { get; set; }
    }
}
