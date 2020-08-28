using System;
using BWR.Application.Dtos.Setting.Coin;

namespace BWR.Application.Dtos.Company.CompanyCashFlow
{
    public class CompanyCashFlowInsertDto 
    {
         public decimal Amount { get; set; }
         public decimal Total { get; set; }
         public bool Matched { get; set; }
         public int CoinId { get; set; }
         public CoinDto Coin { get; set; }
         public int CompanyId { get; set; }
         public CompanyDto Company { get; set; }
         public int? CompanyUserMatched { get; set; }
         public int MoneyActionId { get; set; }
         public Guid UserMatched { get; set; }
         
    }
}
