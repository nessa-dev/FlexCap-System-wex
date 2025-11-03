using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using FlexCap.Web.Data;
using FlexCap.Web.Services;
using FlexCap.Web.Models.Requests;

public class HRApprovalController : Controller
{
    private readonly AppDbContext _context;
    private readonly RequestService _requestService;

    public HRApprovalController(AppDbContext context, RequestService requestService)
    {
        _context = context;
        _requestService = requestService;
    }


    public IActionResult PendingList(string statusFilter, string departmentFilter, string typeFilter)
    {
        var query = _context.Requests.AsQueryable();

        var allAvailableTypes = new List<string> { /* ... sua lista de tipos ... */ };
        var allAvailableDepartments = new List<string> { /* ... sua lista de departamentos ... */ };

        var pendingDbStatuses = new List<string>
    {
        "Waiting For HR",
        "Waiting For Manager",
        "Adjustment Requested"
    };

        var allExistingStatuses = new List<string> { "Approved", "Rejected" };
        allExistingStatuses.AddRange(pendingDbStatuses);

   
        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "Pending")
            {
                query = query.Where(r => pendingDbStatuses.Contains(r.Status));
            }
            else if (statusFilter == "Approved" || statusFilter == "Rejected")
            {
                query = query.Where(r => r.Status == statusFilter);
            }
            else
            {
                query = query.Where(r => r.Status == statusFilter);
            }
        }
        else
        {
            query = query.Where(r => allExistingStatuses.Contains(r.Status));
        }


        var allRequests = query
            .Select(r => new PendingRequestListItemViewModel
            {
                RequestId = r.Id,
                Subject = r.Subject,
                CurrentStatus = r.Status,
                SubmissionDate = r.CreationDate,
                StartDate = r.StartDate,
                CollaboratorName = "Simulated Collaborator Name",
                TypeName = "Simulated Request Type"
            })
            .ToList();

        var viewModel = new ManagerApprovalListViewModel
        {
            PendingRequests = allRequests,
            AvailableTypes = allAvailableTypes,
            AvailableDepartments = allAvailableDepartments
        };

        return View("~/Views/Requests/Rh.cshtml", viewModel);
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessAction(RequestActionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid action. Please provide justification if required.";
            return RedirectToAction(nameof(PendingList));
        }

        try
        {
            var hrUserId = 50; 
            await _requestService.ProcessHRDecision(
                model.RequestId,
                hrUserId,
                model.ActionType,
                model.Justification
            );

            TempData["SuccessMessage"] = $"Action '{model.ActionType}' processed. Request status updated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An unexpected error occurred while processing the request.";
        }
        return RedirectToAction(nameof(PendingList));
    }
}