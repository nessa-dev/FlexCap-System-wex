using FlexCap.Web.Models.Requests;
using System.Collections.Generic;

namespace FlexCap.Web.Models.Requests { 
public class ManagerApprovalListViewModel
{
    public List<PendingRequestListItemViewModel> PendingRequests { get; set; }
    public List<string> AvailableTypes { get; set; } = new List<string>();
    public List<string> AvailableDepartments { get; set; } = new List<string>();
    public string SelectedStatus { get; set; }
    }
}