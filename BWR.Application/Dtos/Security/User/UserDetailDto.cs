using BWR.Application.Dtos.Treasury.UserTreasury;
using System;
using System.Collections.Generic;

namespace BWR.Application.Dtos.Security.User
{
    public class UserDetailDto
    {
        public UserDetailDto()
        {
            UserTreasuries = new List<UserTreasuryDto>();
        }
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }

        public IList<UserTreasuryDto> UserTreasuries { get; set; }
    }
}
