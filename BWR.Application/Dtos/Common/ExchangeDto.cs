using System;
using BWR.Application.Common;

namespace BWR.Application.Dtos.Exchange
{
    public class ExchangeDto : EntityDto
    {
         public decimal AmountOfFirstCoin { get; set; }
         public decimal AmoutOfSecoundCoin { get; set; }
         public decimal? FirstCoinExchangePriceWithMainCoin { get; set; }
         public decimal? FirstCoinSellingPriceWithMainCoin { get; set; }
         public decimal? FirstCoinPurchasingPriceWithMainCoin { get; set; }
         public decimal? SecoundCoinExchangePriceWithMainCoin { get; set; }
         public decimal? SecoundCoinSellingPricWithMainCoin { get; set; }
         public decimal? SecoundCoinPurchasingPriceWithMainCoin { get; set; }
         public string Note { get; set; }
         public int BranchId { get; set; }
         public int FirstCoinId { get; set; }
         public int SecoundCoinId { get; set; }
         public int? MainCoinId { get; set; }
         
    }
}
