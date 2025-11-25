namespace FlexCap.Web.Models.Sprint
{
    public class SprintAbsenceImpactDto
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime? AbsenceStart { get; set; }
        public DateTime? AbsenceEnd { get; set; }
        public double ImpactPercent { get; set; }
        public int AbsentDays { get; set; }
        public int SprintDays { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
