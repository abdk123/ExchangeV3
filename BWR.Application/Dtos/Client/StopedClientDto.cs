using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Client
{
    public class StopedClientDto
    {
        public int ClientId { get; set; }
        public string FullName{ get; set; }
        public string Balnces { get; set; }
        public string LastAction { get; set; }
        public int DayDifference { get; set; }
        public bool IsEnabled { get; set; }
    }
}
