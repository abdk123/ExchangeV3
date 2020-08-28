using System;
using BWR.Application.Common;

namespace BWR.Application.Dtos.BoxAction
{
    public class BoxActionDto : EntityDto
    {
         public decimal Amount { get; set; }
         public bool IsIncmoe { get; set; }
         public string Note { get; set; }
         public int CoinId { get; set; }
         
    }
}
