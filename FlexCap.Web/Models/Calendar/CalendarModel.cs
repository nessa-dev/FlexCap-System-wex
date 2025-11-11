using System;
using System.ComponentModel.DataAnnotations;

namespace FlexCap.Web.Models.Calendar
{
    public class CalendarModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string Type { get; set; } 

        public string Color { get; set; }
    }
}
