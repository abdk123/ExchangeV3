﻿using BWR.Application.Common;
using System.Collections.Generic;

namespace BWR.Application.Dtos.Treasury
{
    public class TreasuryDetailDto:EntityDto
    {
        public TreasuryDetailDto()
        {
            TreasuryCashes = new List<TreasuryCashDto>();
        }

        public string Name { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsAvilable { get; set; }

        public int BranchId { get; set; }

        public IList<TreasuryCashDto> TreasuryCashes { get; set; }
    }
}