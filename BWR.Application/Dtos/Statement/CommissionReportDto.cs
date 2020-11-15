using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Statement
{
    public class CommissionReportDto
    {
        public string CoinName { get; set; }
        public decimal Commission { get; set; }
        public string CompanyName { get; set; }
        public decimal SecondCompanyCommission { get; set; }
        public decimal CompanyCommission { get; set; }
        public string SecondCompanyName { get; set; }
        public decimal AgentCommission { get; set; }
        public string AgentName { get; set; }
        public decimal OurCommission { get; set; }
        public string Date { get; set; }
        public string ReciverName { get; set; }
        public int MoneActionId { get; set; }

    }
}
