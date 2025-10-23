using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // Adicione este using para o IFormFile

namespace FlexCap.Web.Models
{
    public class ColaboradorViewModel
    {
        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        public string? Senha { get; set; }

        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string? Email { get; set; }

        public string? Position { get; set; }
        public string? Department { get; set; }
        public string? TeamName { get; set; }
        public string? Country { get; set; }
        public string? Status { get; set; }

        public IFormFile? PhotoFile { get; set; }
        public string? PhotoUrl { get; set; }
        [NotMapped]
        public bool IsManager => Position == "Project Manager";

    }
}