using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Statement
{
    public class ClearigStatement
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string From { get; set; }
        public string Name { get; set; }
        public string To { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
    }
}
