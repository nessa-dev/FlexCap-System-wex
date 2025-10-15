
using System.ComponentModel.DataAnnotations;

namespace FlexCap.Web.Models.Account
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "The email field is required.")]
        [EmailAddress(ErrorMessage = "The email format is invalid.")]
        [Display(Name = "E-mail Address")]
        public string EmailAddress { get; set; } = string.Empty;
    }
}