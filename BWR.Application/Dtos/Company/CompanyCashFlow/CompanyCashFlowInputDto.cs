using BWR.Application.Common;
using System;

namespace BWR.Application.Dtos.Company.CompanyCashFlow
{
    public class CompanyCashFlowInputDto
    {
        public int CompanyId { get; set; }
        public int CoinId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
