using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BWR.Application.Common;
using BWR.Application.Dtos.Setting.Coin;
using BWR.Domain.Model.Companies;
namespace BWR.Application.Dtos.Company.CompanyCashFlow
{
    public class CompanyCashFlowDto : EntityDto
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
