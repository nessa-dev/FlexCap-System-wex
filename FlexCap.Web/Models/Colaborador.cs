using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlexCap.Web.Models
{
    public class Colaborador
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "URL da Foto")]
        public string PhotoUrl { get; set; } = string.Empty;

        // OBRIGATÓRIO
        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Remote("CheckEmailAvailability", "Colaborador", ErrorMessage = "This email is already registered.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Position is required.")]
        [Display(Name = "Cargo")]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [Display(Name = "Setor")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Team is required.")]
        [Column("Team")]
        [Display(Name = "Time")]
        public string TeamName { get; set; } = string.Empty;

        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }

        [NotMapped]
        public bool IsManager => Position == "Project Manager";
    }
}