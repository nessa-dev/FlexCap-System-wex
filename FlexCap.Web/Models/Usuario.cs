using System.ComponentModel.DataAnnotations;

namespace FlexCap.Web.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        public string Position { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        public string? Team { get; set; }

        [Required]
        public string CountryOfOperation { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}