using System.ComponentModel.DataAnnotations;

namespace FlexCap.Web.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        // CORRIGIDO: Inicializado para resolver CS8618
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        // CORRIGIDO: Inicializado
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        // CORRIGIDO: Inicializado
        public string Position { get; set; } = string.Empty;

        [Required]
        // CORRIGIDO: Inicializado
        public string Department { get; set; } = string.Empty;

        // Propriedade já era anulável ('?'), não precisa de correção.
        public string? Team { get; set; }

        [Required]
        // CORRIGIDO: Inicializado
        public string CountryOfOperation { get; set; } = string.Empty;

        [Required]
        // CORRIGIDO: Inicializado
        public string Password { get; set; } = string.Empty;
    }
}