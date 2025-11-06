using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims; // Necessário para acessar o usuário logado
using Microsoft.EntityFrameworkCore; // Necessário para Include e ToListAsync
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

    // Método Auxiliar para Obter o ID do Usuário Logado (RH)
    private int GetCurrentHRUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(userIdClaim, out int hrId))
        {
            return hrId;
        }
        // Retorna um ID inválido ou lança exceção se o usuário não estiver logado
        return -1;
    }

    // ----------------------------------------------------------------------
    // 1. PENDING LIST (Assíncrona e Com Mapeamento Corrigido)
    // ----------------------------------------------------------------------







    public async Task<IActionResult> PendingList(string statusFilter, string departmentFilter, string typeFilter)
    {
        var query = _context.Requests.AsQueryable();

        // [NOTA]: Você precisará inicializar estas listas com dados reais
        var allAvailableTypes = new List<string> { "Medical Leave", "Day Off" };
        var allAvailableDepartments = new List<string> { "Benefits", "Mobility" };

        var pendingDbStatuses = new List<string>
    {
        "Waiting For HR",
        "Waiting For Manager",
        "Adjustment Requested"
    };

        var allExistingStatuses = new List<string> { "Approved", "Rejected" };
        allExistingStatuses.AddRange(pendingDbStatuses);


        // --- Lógica de Filtro de Status (Mantida) ---
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
            // Padrão: Filtra apenas o que está esperando ação do RH
            query = query.Where(r => r.Status == "Waiting For HR");
        }

        // ----------------------------------------------------
        // 🛑 NOVO: Lógica de Filtro por Departamento
        // ----------------------------------------------------
        if (!string.IsNullOrEmpty(departmentFilter))
        {
            // Filtra pela coluna Department na entidade Colaborador
            query = query.Where(r => r.Colaborador.Department == departmentFilter);
        }

        // ----------------------------------------------------
        // 🛑 NOVO: Lógica de Filtro por Tipo
        // ----------------------------------------------------
        if (!string.IsNullOrEmpty(typeFilter))
        {
            // Filtra pela coluna Name na entidade RequestType
            query = query.Where(r => r.RequestType.Name == typeFilter);
        }
        // ----------------------------------------------------


        // --- Carregamento de Dados Reais e Inclusão (Must be at the end) ---
        query = query
            .Include(r => r.Colaborador)
            .Include(r => r.RequestType) // Assumindo RequestType é a propriedade correta
            .OrderByDescending(r => r.CreationDate);


        var allRequests = await query
            .Select(r => new PendingRequestListItemViewModel
            {
                RequestId = r.Id,
                Subject = r.Subject,
                CurrentStatus = r.Status,
                SubmissionDate = r.CreationDate,
                StartDate = r.StartDate,

                // Mapeamento de dados reais do colaborador:
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


    [HttpGet]
    public async Task<IActionResult> GetRequestDetails(int requestId)
    {
        try
        {
            var request = await _context.Requests
                .Include(r => r.Colaborador)
                .Include(r => r.RequestType) // 🛑 ADICIONE ESTE INCLUDE AQUI!
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



    // ----------------------------------------------------------------------
    // 2. PROCESS ACTION (Correto para RH)
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
            // Usar o ID do usuário RH logado
            var hrUserId = GetCurrentHRUserId();

            if (hrUserId == -1)
            {
                throw new InvalidOperationException("User not authenticated or ID is invalid.");
            }

            await _requestService.ProcessHRDecision(
                model.RequestId,
                hrUserId, // ID real
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