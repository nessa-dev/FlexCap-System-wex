using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlexCap.Web.Models
{
    public class ColaboradorViewModel
    {
        public int Id { get; set; }

        
        [DataType(DataType.Password)]
        public string? Senha { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Position is required.")]
        public string? Position { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Team Name is required.")]
        public string? TeamName { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public string? Country { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string? Status { get; set; }

        [Display(Name = "Reason for Inactivity")]
        public string? InactivityReason { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Expected Return Date")]
        public DateTime? EndDate { get; set; }

        public IFormFile? PhotoFile { get; set; }

        public string? PhotoUrl { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Inactivity Start Date")]
        public DateTime? StartDate { get; set; }

        [NotMapped]
        public bool IsManager => Position == "Project Manager";
    }
}