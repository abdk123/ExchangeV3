using System;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.User
{
    public class UserUpdateDto
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "«·«”„ «·ﬂ«„· „ÿ·Ê»")]
        [Display(Name = "«·«”„ «·ﬂ«„·")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "«”„ «·„” Œœ„ „ÿ·Ê»")]
        [Display(Name = "«”„ «·„” Œœ„")]
        public string Username { get; set; }

        [Display(Name = "ﬂ·„… «·„—Ê—")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "ﬂ·„… «·„—Ê— ÌÃ» √‰  ﬂÊ‰ 6 „Õ«—› ⁄·Ï «·«ﬁ·")]
        public string PasswordHash { get; set; }

        [Display(Name = " √ﬂÌœ ﬂ·„… «·„—Ê—")]
        [DataType(DataType.Password)]
        [Compare("PasswordHash", ErrorMessage = "ÌÃ» √‰ ÌﬂÊ‰ „ÿ«»ﬁ ·Õﬁ· ﬂ·„… «·„—Ê—")]
        public string ConfirmPassword { get; set; }

    }
}
