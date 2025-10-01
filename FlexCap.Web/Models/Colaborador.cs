using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlexCap.Web.Models
{
    public class Colaborador
    {
        [Key]
        public int Id { get; set; }
        // CORRIGIDO: Inicializado para resolver CS8618
        public string FotoUrl { get; set; } = string.Empty;
        // CORRIGIDO: Inicializado
        public string NomeCompleto { get; set; } = string.Empty;
        // CORRIGIDO: Inicializado
        public string Email { get; set; } = string.Empty;
        // CORRIGIDO: Inicializado
        public string Cargo { get; set; } = string.Empty;
        // CORRIGIDO: Inicializado
        public string Setor { get; set; } = string.Empty;
        // CORRIGIDO: Inicializado
        public string Status { get; set; } = string.Empty;
        // CORRIGIDO: Inicializado
        public string Pais { get; set; } = string.Empty;

        [Column("Team")]
        // CORRIGIDO: Inicializado
        public string Time { get; set; } = string.Empty;

    }
}