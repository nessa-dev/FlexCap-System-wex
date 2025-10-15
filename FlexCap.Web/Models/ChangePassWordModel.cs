//Page mudar a senha após o login
using System.ComponentModel.DataAnnotations;

namespace FlexCap.Web.Models.Account
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "The current password field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? OldPassword { get; set; }

        [Required(ErrorMessage = "The new password field is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmNewPassword { get; set; }
    }
}