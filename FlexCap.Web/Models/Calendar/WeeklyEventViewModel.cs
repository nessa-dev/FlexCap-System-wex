// Em FlexCap.Web.Models.Calendar/WeeklyEventViewModel.cs (ou onde preferir)

using System;

namespace FlexCap.Web.Models.Calendar
{
    public class WeeklyEventViewModel
    {
        public string Title { get; set; }
        public string DayOfWeekShort { get; set; } // e.g., "Tue", "Thu"
        public string IconClass { get; set; } // e.g., "bi-calendar-check"
    }
}