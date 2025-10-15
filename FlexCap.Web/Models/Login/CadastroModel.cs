using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; 

namespace FlexCap.Web.Models.Account
{
    public class CadastroModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "The Profile Photo field is required.")]
        public IFormFile ProfilePhotoFile { get; set; }

        [Required(ErrorMessage = "The Full Name field is required.")]
        [MinLength(1, ErrorMessage = "The Full Name field is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Email Address field is required.")]
        [EmailAddress(ErrorMessage = "The email format is invalid.")]
        [MinLength(1, ErrorMessage = "The Email Address field is required.")]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Position field is required.")]
        [MinLength(1, ErrorMessage = "The Position field is required.")]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Department field is required.")]
        [MinLength(1, ErrorMessage = "The Department field is required.")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Team field is required.")]
        [MinLength(1, ErrorMessage = "The Team field is required.")]
        public string Team { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Country of Operation field is required.")]
        [MinLength(1, ErrorMessage = "The Country of Operation field is required.")]
        [Display(Name = "Country of Operation")]
        public string CountryOfOperation { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Password field is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
        [Display(Name = "Create Password")]
        public string Password { get; set; } = string.Empty;
    }
}