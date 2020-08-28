using System.ComponentModel.DataAnnotations;

namespace Bwr.WebApp.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage ="اسم المستخدم مطلوب")]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required(ErrorMessage ="كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.EmailAddress,ErrorMessage ="Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الحالية")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "كلمة المرور يجب أن تكون 6 محارف على الاقل")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الجديدة")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور غير متطابقة")]
        public string ConfirmPassword { get; set; }
    }
}
