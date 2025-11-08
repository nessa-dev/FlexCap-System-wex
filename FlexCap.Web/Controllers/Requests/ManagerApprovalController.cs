using FlexCap.Web.Data;
using FlexCap.Web.Models.Requests;
using FlexCap.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims; 
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; 

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


    public async Task<IActionResult> DownloadAttachment(int requestId, [FromServices] IWebHostEnvironment env)
    {
        var request = await _context.Requests.FindAsync(requestId);

        if (request == null || string.IsNullOrEmpty(request.AttachmentPath))
        {
            return NotFound("Anexo não encontrado ou caminho não especificado.");
        }

        // Usamos a variável 'env' (sem underscore) injetada no método
        var uploadsFolder = Path.Combine(env.ContentRootPath, "Uploads");

        var filePath = Path.Combine(uploadsFolder, request.AttachmentPath);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("O arquivo de anexo não foi encontrado no servidor.");
        }

        var mimeType = "application/pdf";
        var fileName = Path.GetFileName(request.AttachmentPath);

        // Retorna o arquivo (FileResult)
        return File(filePath, mimeType, fileName);
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



    [HttpGet]
    public async Task<IActionResult> GetRequestDetails(int requestId)
    {
        try
        {
            var request = await _context.Requests
                .Include(r => r.Colaborador)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                return Json(new { error = "Request details not found." });
            }

            var result = new
            {
                requestId = request.Id,
                subject = request.Subject,
                description = request.Description,
                startDate = request.StartDate.ToShortDateString(),
                endDate = request.EndDate.ToShortDateString(),
                attachmentPath = request.AttachmentPath,
                collaboratorName = request.Colaborador?.FullName,
                collaboratorPosition = request.Colaborador?.Position,
                collaboratorDepartment = request.Colaborador?.Department,
                currentStatus = request.Status
            };

            return Json(result);
        }
        catch (Exception ex)
        {
            return Json(new { error = $"Erro ao buscar detalhes: {ex.Message}" });
        }
    }



    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var request = await _context.Requests
            .Include(r => r.Colaborador)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound();

        return View("Detail", request); 
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessAction(RequestActionViewModel model)
    {
        // 🔹 Validação do model base
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid action. Please provide justification if required.";
            return RedirectToAction(nameof(PendingList));
        }

        try
        {
            var managerId = GetCurrentManagerId();

            if (managerId <= 0)
            {
                throw new UnauthorizedAccessException("Usuário Manager não autenticado ou ID inválido.");
            }

            if (model.ActionType.Equals("Reject", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(model.Justification))
            {
                TempData["ErrorMessage"] = "Please provide a justification for rejection.";
                return RedirectToAction(nameof(PendingList));
            }

            await _requestService.ProcessManagerDecision(
                model.RequestId,
                managerId,
                model.ActionType,
                model.Justification
            );

            if (model.ActionType.Equals("Approve", StringComparison.OrdinalIgnoreCase))
            {
                TempData["SuccessMessage"] = "Request approved and forwarded to HR.";
            }
            else if (model.ActionType.Equals("Reject", StringComparison.OrdinalIgnoreCase))
            {
                TempData["SuccessMessage"] = "Request rejected and returned to collaborator.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Action '{model.ActionType}' processed successfully.";
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = $"Operation error: {ex.Message}";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Unexpected error: {ex.Message}";
        }
        return RedirectToAction(nameof(PendingList));
    }




}