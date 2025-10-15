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

        [Display(Name = "Nome Completo")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        [Display(Name = "Cargo")]
        public string Position { get; set; } = string.Empty;

        [Display(Name = "Setor")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        [Column("Team")]
        [Display(Name = "Time")]
        public string TeamName { get; set; } = string.Empty;
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }


    }
}