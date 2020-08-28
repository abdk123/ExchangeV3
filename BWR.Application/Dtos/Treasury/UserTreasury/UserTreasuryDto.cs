using BWR.Application.Common;
using System;

namespace BWR.Application.Dtos.Treasury.UserTreasury
{
    public class UserTreasuryDto: EntityDto
    {
        public DateTime? DeliveryDate { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public int TreasuryId { get; set; }
        public string TreasuryName { get; set; }
        public Guid UserId { get; set; }
    }
}
