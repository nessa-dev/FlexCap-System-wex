namespace FlexCap.Web.Models.Sprint
{
    public class SprintResultModel
    {
        public int Id { get; set; }
        public int SprintId { get; set; }
        public string Name { get; set; }
        public string Goal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public int CompletionPercentage { get; set; } 
        public string Blockers { get; set; }
        public string WorkedWell { get; set; }
        public string Improvement { get; set; }
        public DateTime ActualFinishDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Completed";
    }
}