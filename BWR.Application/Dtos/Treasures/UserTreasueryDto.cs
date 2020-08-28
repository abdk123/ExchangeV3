using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BWR.Domain.Model.Treasures;
namespace BWR.Application.Dtos.UserTreasuery
{
    public class UserTreasueryDto.cs : EntityDto
    {
         public int TreasuryId { get; set; }
         public TreasuryDto Treasury { get; set; }
         public Guid UserId { get; set; }
         public UserDto User { get; set; }
    }
}
