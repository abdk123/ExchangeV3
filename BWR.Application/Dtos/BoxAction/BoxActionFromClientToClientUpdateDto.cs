using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.BoxAction
{
    public class BoxActionFromClientToClientUpdateDto
    {
        public int MoneyActionId { get; set; }
        public int CoinId { get; set; }
        public int FirstClientId { get; set; }
        public int SecondClientId { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
    }
}
