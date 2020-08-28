using BWR.Application.Common;
using System.Collections.Generic;

namespace BWR.Application.Dtos.Treasury
{
    public class TreasurysDto:EntityDto
    {
        public string Name { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsAvilable { get; set; }

        public string Balances { get; set; }
    }
}
