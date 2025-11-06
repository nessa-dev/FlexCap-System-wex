using FlexCap.Web.Models.Requests;
using System.ComponentModel.DataAnnotations;

namespace FlexCap.Web.Models
{
    public class RequestType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } 

        public ICollection<RequestEntity> Requests { get; set; }
    }
}