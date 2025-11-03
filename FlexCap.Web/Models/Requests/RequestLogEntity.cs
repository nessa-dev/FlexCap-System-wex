using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlexCap.Web.Models.Requests
{
    public class RequestLogEntity
    {
        [Key]
        public int Id { get; set; }

        public int RequestId { get; set; } 

        [Required]
        public int ActionByUserId { get; set; } 

        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } 

        [Required]
        [StringLength(50)]
        public string NewStatus { get; set; } 

        public DateTime ActionTimestamp { get; set; }

        public string Comment { get; set; }
    }
}