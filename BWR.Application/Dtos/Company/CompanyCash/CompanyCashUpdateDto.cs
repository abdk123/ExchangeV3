using BWR.Application.Dtos.Setting.Coin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Company
{
    public class CompanyCashUpdateDto
    {
        public decimal InitialBalance { get; set; }
        public decimal Total { get; set; }
        public int CoinId { get; set; }
        public int CompanyId { get; set; }
        public CoinDto Coin { get; set; }
    }
}
