using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlexCap.Web.Models.Requests
{
    public class RequestEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        public int TypeId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public string AttachmentPath { get; set; }
        [Required]
        [Column("CollaboratorId")] 
        public int CollaboratorId { get; set; }
        [ForeignKey(nameof(CollaboratorId))]
        public Colaborador Colaborador { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } 

        public DateTime CreationDate { get; set; }

        public DateTime? LastUpdateDate { get; set; } 

        public int? CurrentHRId { get; set; }

        public int? CurrentManagerId { get; set; }

        public string? RejectionReason { get; set; }
    }
}