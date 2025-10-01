using System.ComponentModel.DataAnnotations;

namespace FlexCap.Web.Models.Account
{
    public class CadastroModel
    {
        [Key] // Chave primária
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo Nome Completo é obrigatório.")]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "O campo E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
        [Display(Name = "E-mail Address")]
        public string? EmailAddress { get; set; }

        [Required(ErrorMessage = "O campo Cargo é obrigatório.")]
        public string? Position { get; set; }

        [Required(ErrorMessage = "O campo Departamento é obrigatório.")]
        public string? Department { get; set; }

        public string? Team { get; set; }

        [Required(ErrorMessage = "O campo País de Operação é obrigatório.")]
        [Display(Name = "Country of Operation")]
        public string? CountryOfOperation { get; set; }

        [Required(ErrorMessage = "O campo Senha é obrigatória.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [Display(Name = "Create Password")]
        public string? Password { get; set; }
    }
}
