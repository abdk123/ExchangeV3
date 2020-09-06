using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWR.Application.Common;

namespace BWR.Application.Dtos.Client.ClientCashFlow
{
    public class ClientCashFlowOutputDto : EntityDto
    {
        public decimal? Balance { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Commission { get; set; }
        public decimal? SecondCommission { get; set; }
        public string Type { get; set; }
        //public string Name { get; set; }
        public int? Number { get; set; }
        public string Date { get; set; }
        public string Note { get; set; }
        public int? MoneyActionId { get; set; }


    }
}
