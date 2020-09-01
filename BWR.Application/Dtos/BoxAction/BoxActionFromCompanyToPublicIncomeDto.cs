using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.BoxAction
{
    public class BoxActionFromCompanyToPublicIncomeDto
    {
        public int CoinId { get; set; }
        public int ClientId { get; set; }
        public int PublicIncomeId { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
    }
}
