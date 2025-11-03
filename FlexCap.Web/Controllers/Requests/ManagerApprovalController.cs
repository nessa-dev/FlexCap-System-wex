using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims; 
using FlexCap.Web.Data;
using FlexCap.Web.Services;
using FlexCap.Web.Models.Requests;

public class ManagerApprovalController : Controller
{
    private readonly AppDbContext _context;
    private readonly RequestService _requestService;

    public ManagerApprovalController(AppDbContext context, RequestService requestService)
    {
        _context = context;
        _requestService = requestService;
    }

    private int GetCurrentManagerId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(userIdClaim, out int managerId))
        {
            return managerId;
        }
        return -1;
    }

    
    public IActionResult PendingList()
    {
        var pendingStatus = "Waiting For Manager";

        var managerId = GetCurrentManagerId();

        if (managerId <= 0)
        {
            return View("~/Views/Requests/Manager.cshtml", new ManagerApprovalListViewModel());
        }

        var pendingRequests = _context.Requests
            .Where(r => r.Status == pendingStatus && r.CurrentManagerId == managerId)
            .Select(r => new PendingRequestListItemViewModel
            {
                RequestId = r.Id,
                Subject = r.Subject,
                SubmissionDate = r.CreationDate,
                StartDate = r.StartDate,

                CollaboratorName = r.Colaborador.FullName,
                TypeName = r.Colaborador.Department, 
                Position = r.Colaborador.Position,
                Department = r.Colaborador.Department
            })
            .ToList();

        var viewModel = new ManagerApprovalListViewModel
        {
            PendingRequests = pendingRequests
        };

        return View("~/Views/Requests/Manager.cshtml", viewModel);
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
            var managerId = GetCurrentManagerId();

            if (managerId <= 0) throw new UnauthorizedAccessException("Usuário Manager não autenticado ou ID inválido.");

            await _requestService.ProcessManagerDecision(
                model.RequestId,
                managerId,
                model.ActionType,
                model.Justification
            );

            TempData["SuccessMessage"] = $"Action '{model.ActionType}' processed. Request forwarded to HR.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(PendingList));
    }
}