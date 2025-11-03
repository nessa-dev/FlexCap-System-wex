
using FlexCap.Web.Models.Requests;
using System.Collections.Generic;

namespace FlexCap.Web.Models 
{
    public class HomeMetricsViewModel
    {
        public int TotalPendingHR { get; set; }
        public int TotalEmployees { get; set; }
        public int ActiveSectorsCount { get; set; }
        public List<PendingRequestListItemViewModel> LatestPendingRequests { get; set; } = new List<PendingRequestListItemViewModel>();
    }
}