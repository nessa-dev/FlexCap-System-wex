using FlexCap.Web.Models.Requests;
using System.Collections.Generic;

namespace FlexCap.Web.Models
{
    public class HomeMetricsViewModel
    {
        public int ActiveSprintCount { get; set; }

        public bool IsSprintActive { get; set; } 
        public string SprintName { get; set; } 
        public string SprintDuration { get; set; } 
        public double SprintProgressPercent { get; set; } 
        public List<string> SprintImpactNotifications { get; set; } = new List<string>(); 

        public int TotalPendingHR { get; set; }
        public int TotalEmployees { get; set; }
        public int ActiveSectorsCount { get; set; }

        public List<PendingRequestListItemViewModel> LatestPendingRequests { get; set; } = new List<PendingRequestListItemViewModel>();

        public string FirstName { get; set; } = "Usuário";
        public string UserTeam { get; set; } = "Sem Time";
        public int ActiveMembers { get; set; }
        public int TotalMembers { get; set; }

        public List<PendingRequestListItemViewModel> ColaboratorHistory { get; set; } = new List<PendingRequestListItemViewModel>();
        public List<PendingRequestListItemViewModel> PendingManagerRequests { get; set; } = new List<PendingRequestListItemViewModel>();
    }
}