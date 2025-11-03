using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FlexCap.Web.Models.Requests
{
    public class AbsenceRequestSubmitViewModel
    {
        [Required(ErrorMessage = "The subject is mandatory.")]
        [StringLength(100, ErrorMessage = "The subject must be at most 100 characters.")]
        public string Subject { get; set; }
        [Required(ErrorMessage = "Select the request type.")]
        [Display(Name = "Request Type")]
        public int TypeId { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "The description is mandatory.")]
        [StringLength(500, ErrorMessage = "The description must be at most 500 characters.")]
        public string Description { get; set; }

        [Display(Name = "Attachment File")]
        public IFormFile AttachmentFile { get; set; }
    }
}