using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // <-- ADICIONADO para o [Remote]

namespace FlexCap.Web.Models
{
    public class ColaboradorViewModel
    {
        // SENHA
        [Required(ErrorMessage = "Password is required.")] // Alterado para inglês
        [DataType(DataType.Password)]
        public string? Senha { get; set; }

        // NOME COMPLETO
        [Required(ErrorMessage = "Full Name is required.")] // Alterado para inglês
        public string? FullName { get; set; }

        // EMAIL - OBRIGATÓRIO, FORMATO E UNICIDADE
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        // CORREÇÃO AQUI: 'Registro' é o nome da Controller que contém a Action.
        [Remote("CheckEmailAvailability", "Registro", ErrorMessage = "This email is already registered.")]
        public string? Email { get; set; }

        // CARGO
        [Required(ErrorMessage = "Position is required.")]
        public string? Position { get; set; }

        // SETOR
        [Required(ErrorMessage = "Department is required.")]
        public string? Department { get; set; }

        // TIME
        [Required(ErrorMessage = "Team Name is required.")]
        public string? TeamName { get; set; }

        // PAÍS
        [Required(ErrorMessage = "Country is required.")]
        public string? Country { get; set; }

        // STATUS (Geralmente obrigatório, mas opcional se for definido por padrão no código)
        // Deixei como Required, ajuste se não for obrigatório no seu fluxo
        [Required(ErrorMessage = "Status is required.")]
        public string? Status { get; set; }

        // ARQUIVO DE FOTO (Geralmente opcional no cadastro inicial)
        public IFormFile? PhotoFile { get; set; }

        public string? PhotoUrl { get; set; }

        [NotMapped]
        public bool IsManager => Position == "Project Manager";

    }
}