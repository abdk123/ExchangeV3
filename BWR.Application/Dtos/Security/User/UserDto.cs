using System;

namespace BWR.Application.Dtos.User
{
    public class UserDto 
    {
         public Guid UserId { get; set; }
         public string FullName { get; set; }
         public string Username { get; set; }
         public string PasswordHash { get; set; }
         public string SecurityStamp { get; set; }
         public string Email { get; set; }
         public string ImageUrl { get; set; }
         
    }
}
