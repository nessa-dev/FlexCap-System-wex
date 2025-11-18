
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FlexCap.Web.Models
{
    public class SprintModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Sprint Name is required.")]
        public string Name { get; set; } = string.Empty;

        public string? Goal { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public string? Notes { get; set; }

        public string? ParticipatingMemberIds { get; set; }

        public bool IsActive { get; set; } = true;

    }
}