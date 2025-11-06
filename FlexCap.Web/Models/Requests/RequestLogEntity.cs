using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FlexCap.Web.Models.Requests
{
    public class RequestLogEntity
    {
        [Key]
        public int Id { get; set; }

        public int RequestId { get; set; }

        [ForeignKey(nameof(RequestId))]
        public RequestEntity Request { get; set; }

        [Required]
        public int ActionByUserId { get; set; }

        [ForeignKey(nameof(ActionByUserId))]
        public Colaborador ActionByUser { get; set; }

        [Required]
        [StringLength(50)]
        public string ActionType { get; set; }

        [Required]
        [StringLength(50)]
        public string NewStatus { get; set; }

        public DateTime ActionTimestamp { get; set; }

        public string? Comment { get; set; }
    }
}