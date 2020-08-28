using System;
using BWR.Application.Common;
using BWR.Application.Dtos.Setting.Coin;
using BWR.Application.Dtos.Setting.Country;

namespace BWR.Application.Dtos.Branch
{
    public class BranchCommissionDto : EntityDto
    {
         public decimal StartRange { get; set; }
         public decimal EndRange { get; set; }
         public decimal Cost { get; set; }
         public decimal Ratio { get; set; }
         public bool IsEnabled { get; set; }
         public int BranchId { get; set; }
         public BranchDto Branch { get; set; }
         public int CoinId { get; set; }
         public CoinDto Coin { get; set; }
         public int CountryId { get; set; }
         public CountryDto Country { get; set; }
    }
}
