using System;

namespace FlexCap.Web.Models.Requests
{
    public class PendingRequestListItemViewModel
    {
        public int RequestId { get; set; }

        public string CollaboratorName { get; set; } = string.Empty;

        public string Position { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;


        public string Subject { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string CurrentStatus { get; set; } = string.Empty;

        public string DisplayStatus
        {
            get
            {
                return CurrentStatus switch
                {
                    "Waiting For HR" => "Pending",
                    "Waiting For Manager" => "Pending",
                    "Adjustment Requested" => "Pending",
                    "Approved" => "Approved",
                    "Rejected" => "Rejected",
                    _ => "Unknown"
                };
            }
        }
    }
}