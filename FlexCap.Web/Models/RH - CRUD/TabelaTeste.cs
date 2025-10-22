// FlexCap.Web.Models/TabelaTeste.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlexCap.Web.Models
{
    public class TabelaTeste
    {
        // Chave Primária (necessária para Entity Framework Core)
        [Key]
        public int Id { get; set; }

        // Campo para o nome do novo dado
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public string? NomeDoDado { get; set; }

        // Campo de exemplo (um número ou quantidade)
        [Required(ErrorMessage = "A quantidade é obrigatória.")]
        public int Quantidade { get; set; }

        // Um campo de data para saber quando foi criado (opcional)
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}