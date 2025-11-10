using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FlexCap.Web.Data;
using FlexCap.Web.Models.Requests;
using FlexCap.Web.Services;
using Microsoft.EntityFrameworkCore;

public class RequestController : Controller
{
    private readonly RequestService _requestService;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public RequestController(RequestService requestService, AppDbContext context, IWebHostEnvironment env)
    {
        _requestService = requestService;
        _context = context;
        _env = env;
    }


    public async Task<IActionResult> Submit()
    {
        var collaboratorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(collaboratorIdString) || !int.TryParse(collaboratorIdString, out int collaboratorId))
        {
            return View("~/Views/Requests/Colaborador.cshtml", new SubmitViewModelWithHistory());
        }

        return await ReturnSubmitViewWithHistory(new AbsenceRequestSubmitViewModel());
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(AbsenceRequestSubmitViewModel model)
    {
        // Validações básicas
        if (!model.StartDate.HasValue)
        {
            ModelState.AddModelError(nameof(model.StartDate), "The start date is mandatory.");
        }
        if (!model.EndDate.HasValue)
        {
            ModelState.AddModelError(nameof(model.EndDate), "The end date is mandatory.");
        }
        if (model.EndDate.HasValue && model.StartDate.HasValue && model.EndDate.Value < model.StartDate.Value)
        {
            ModelState.AddModelError(nameof(model.EndDate), "The end date cannot be earlier than the start date.");
        }

        if (!ModelState.IsValid)
        {
            return await ReturnSubmitViewWithHistory(model);
        }

        try
        {
            var collaboratorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(collaboratorIdString) || !int.TryParse(collaboratorIdString, out int collaboratorId))
            {
                throw new InvalidOperationException("User ID could not be determined or is invalid.");
            }

            // ✅ Agora o arquivo vem direto do model.AttachmentFile
            await _requestService.SubmitNewRequest(model, collaboratorId);

            TempData["SuccessMessage"] = "Request submitted successfully! It is awaiting Manager approval.";
            return RedirectToAction(nameof(Submit));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return await ReturnSubmitViewWithHistory(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An unexpected error occurred while saving your request: {ex.Message}");
            return await ReturnSubmitViewWithHistory(model);
        }
    }







    private async Task<List<PendingRequestListItemViewModel>> GetRequestHistoryAsync(int collaboratorId)
    {
        // Adicionar Include para carregar RequestType se o TypeName na ViewModel precisar dele
        return await _context.Requests
            .Include(r => r.Colaborador)
            .Include(r => r.RequestType) // Adicione este include se você mapear r.RequestType.Name
            .Where(r => r.CollaboratorId == collaboratorId)
            .OrderByDescending(r => r.CreationDate)
            .Select(r => new PendingRequestListItemViewModel
            {
                Subject = r.Subject,
                SubmissionDate = r.CreationDate,
                CurrentStatus = r.Status,

                // 🛑 CORREÇÃO CRÍTICA: Mapear o motivo de rejeição/ajuste
                RejectionReason = r.RejectionReason,

                // Mapeamentos para a tabela de histórico (opcional, mas bom)
                StartDate = r.StartDate,
                EndDate = r.EndDate,

                // Mapeamentos existentes:
                CollaboratorName = r.Colaborador.FullName,
                TypeName = r.RequestType.Name // Assumindo que você quer o nome do tipo
            })
            .ToListAsync();
    }






    private async Task<IActionResult> ReturnSubmitViewWithHistory(AbsenceRequestSubmitViewModel modelWithErrors)
    {
        var collaboratorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(collaboratorIdString, out int collaboratorId);

        var history = await GetRequestHistoryAsync(collaboratorId);

        var errorViewModel = new SubmitViewModelWithHistory
        {
            Subject = modelWithErrors.Subject,
            TypeId = modelWithErrors.TypeId,
            StartDate = modelWithErrors.StartDate,
            EndDate = modelWithErrors.EndDate,
            Description = modelWithErrors.Description,
            RequestHistory = history
        };

        return View("~/Views/Requests/Colaborador.cshtml", errorViewModel);
    }
}