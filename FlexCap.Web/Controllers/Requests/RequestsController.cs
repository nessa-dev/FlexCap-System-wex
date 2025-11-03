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

    public RequestController(RequestService requestService, AppDbContext context)
    {
        _requestService = requestService;
        _context = context; 
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
            var collaboratorIdString = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(collaboratorIdString) || !int.TryParse(collaboratorIdString, out int collaboratorId))
            {
                throw new InvalidOperationException("User ID could not be determined or is invalid.");
            }

            await _requestService.SubmitNewRequest(model, collaboratorId);

            TempData["SuccessMessage"] = "Request submitted successfully! It is awaiting Manager approval.";

            return RedirectToAction(nameof(Submit));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);

            return await ReturnSubmitViewWithHistory(model);
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "An unexpected error occurred while saving your request.");
            return await ReturnSubmitViewWithHistory(model);
        }
    }






    private async Task<List<PendingRequestListItemViewModel>> GetRequestHistoryAsync(int collaboratorId)
    {
        return await _context.Requests
            .Include(r => r.Colaborador)
            .Where(r => r.CollaboratorId == collaboratorId)
            .OrderByDescending(r => r.CreationDate)
            .Select(r => new PendingRequestListItemViewModel
            {
                Subject = r.Subject,
                SubmissionDate = r.CreationDate,
                CurrentStatus = r.Status,
                CollaboratorName = r.Colaborador.FullName 
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