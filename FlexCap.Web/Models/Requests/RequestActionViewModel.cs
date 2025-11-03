using System.ComponentModel.DataAnnotations;

namespace FlexCap.Web.Models.Requests
{
    public class RequestActionViewModel
    {
        [Required]
        public int RequestId { get; set; }

        [Required]
        public string ActionType { get; set; }

        [StringLength(500)]
        public string Justification { get; set; }
    }
}