
using FlexCap.Web.Models.Requests;
using System.Collections.Generic;

namespace FlexCap.Web.Models.Requests
{
    public class SubmitViewModelWithHistory : AbsenceRequestSubmitViewModel
    {
        public List<PendingRequestListItemViewModel> RequestHistory { get; set; } = new List<PendingRequestListItemViewModel>();
    }
}