namespace FlexCap.Web.Models.Sprint
{
    public class SprintHolidayNotificationDto
    {

        public int EventId { get; set; }
        public string EventTitle { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = "";
        public string IconClass { get; set; } = "";
        public int AffectedMembers { get; set; }
        public int TotalMembers { get; set; }
        public double AffectedPercent { get; set; } 
        public string CountryKey { get; set; } = "";
        public string Message { get; set; } = "";

    }
}
