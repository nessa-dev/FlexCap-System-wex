using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
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

    private int GetCurrentHRUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out int hrId) ? hrId : -1;
    }

    // ----------------------------------------------------------------------
    // 1. PENDING LIST (Filtragem dinâmica e mapeamento completo)
    // ----------------------------------------------------------------------
    public async Task<IActionResult> PendingList(string statusFilter, string departmentFilter, string typeFilter)
    {
        var query = _context.Requests.AsQueryable();

        var allAvailableTypes = new List<string> { "Medical Leave", "Day Off" };
        var allAvailableDepartments = new List<string> { "Benefits", "Mobility" };

        var pendingDbStatuses = new List<string>
        {
            "Waiting For HR",
            "Waiting For Manager",
            "Adjustment Requested"
        };

        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "Pending")
                query = query.Where(r => pendingDbStatuses.Contains(r.Status));
            else
                query = query.Where(r => r.Status == statusFilter);
        }
        else
        {
            query = query.Where(r => r.Status == "Waiting For HR");
        }

        if (!string.IsNullOrEmpty(departmentFilter))
            query = query.Where(r => r.Colaborador.Department == departmentFilter);

        if (!string.IsNullOrEmpty(typeFilter))
            query = query.Where(r => r.RequestType.Name == typeFilter);

        query = query
            .Include(r => r.Colaborador)
            .Include(r => r.RequestType)
            .OrderByDescending(r => r.CreationDate);

        var allRequests = await query
            .Select(r => new PendingRequestListItemViewModel
            {
                RequestId = r.Id,
                Subject = r.Subject,
                CurrentStatus = r.Status,
                SubmissionDate = r.CreationDate,
                StartDate = r.StartDate,
                CollaboratorName = r.Colaborador.FullName,
                TypeName = r.RequestType.Name,
                Position = r.Colaborador.Position,
                Department = r.Colaborador.Department
            })
            .ToListAsync();

        var viewModel = new ManagerApprovalListViewModel
        {
            PendingRequests = allRequests,
            AvailableTypes = allAvailableTypes,
            AvailableDepartments = allAvailableDepartments
        };

        return View("~/Views/Requests/Rh.cshtml", viewModel);
    }

    // ----------------------------------------------------------------------
    // 2. GET DETAILS
    // ----------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> GetRequestDetails(int requestId)
    {
        try
        {
            var request = await _context.Requests
                .Include(r => r.Colaborador)
                .Include(r => r.RequestType)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return Json(new { error = "Request details not found." });

            return Json(new
            {
                requestId = request.Id,
                subject = request.Subject,
                description = request.Description,
                startDate = request.StartDate.ToString("yyyy-MM-dd"),
                endDate = request.EndDate.ToString("yyyy-MM-dd"),
                attachmentPath = request.AttachmentPath,
                collaboratorName = request.Colaborador?.FullName,
                collaboratorPosition = request.Colaborador?.Position,
                collaboratorDepartment = request.Colaborador?.Department,
                typeName = request.RequestType?.Name,
                currentStatus = request.Status
            });
        }
        catch (Exception ex)
        {
            return Json(new { error = $"Erro ao buscar detalhes: {ex.Message}" });
        }
    }

    // ----------------------------------------------------------------------
    // 3. PROCESS ACTION
    // ----------------------------------------------------------------------





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
            var hrId = GetCurrentHRUserId();

            if (hrId <= 0)
                throw new UnauthorizedAccessException("Usuário RH não autenticado ou ID inválido.");

            if (model.ActionType.Equals("Reject", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(model.Justification))
            {
                TempData["ErrorMessage"] = "Please provide a justification for rejection.";
                return RedirectToAction(nameof(PendingList));
            }

            await _requestService.ProcessHRDecision(
                model.RequestId,
                hrId,
                model.ActionType,
                model.Justification
            );

            if (model.ActionType.Equals("Approve", StringComparison.OrdinalIgnoreCase))
                TempData["SuccessMessage"] = "Request approved successfully.";
            else if (model.ActionType.Equals("Reject", StringComparison.OrdinalIgnoreCase))
                TempData["SuccessMessage"] = "Request rejected and returned to collaborator.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Unexpected error: {ex.Message}";
        }

        return RedirectToAction(nameof(PendingList));
    }









}
