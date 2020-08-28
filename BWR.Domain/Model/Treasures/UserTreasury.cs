using BWR.Domain.Model.Security;
using BWR.ShareKernel.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Treasures
{
    public class UserTreasuery: Entity
    {
        public DateTime? DeliveryDate { get; set; }

        public int TreasuryId { get; set; }
        [ForeignKey("TreasuryId")]
        public virtual Treasury Treasury { get; set; }

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}