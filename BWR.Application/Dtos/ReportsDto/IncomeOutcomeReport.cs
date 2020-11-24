using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.ReportsDto
{
    public class IncomeOutcomeReport
    {
        public int MoneyActionId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Note { get; set; }
    }
}
