using FlexCap.Web.Data;
using FlexCap.Web.Models;
using FlexCap.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FlexCap.Web.Models.Requests;
using FlexCap.Web.Models.Sprint; 

namespace FlexCap.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly RequestService _requestService;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, RequestService requestService)
        {
            _logger = logger;
            _context = context;
            _requestService = requestService;
        }

        private async Task<SprintModel> GetActiveTeamSprint(Colaborador loggedUser)
        {
            if (loggedUser.TeamName == null) return null;

            // Identifica os IDs dos membros da equipe do usuário logado
            var teamMemberIds = await _context.Colaboradores
                .Where(c => c.TeamName == loggedUser.TeamName)
                .Select(c => c.Id)
                .ToListAsync();

            var teamMemberIdStrings = teamMemberIds.Select(i => i.ToString()).ToList();

            // Busca todas as sprints ATIVAS
            var activeSprints = await _context.Sprints
                .Where(s => s.IsActive == true)
                .ToListAsync();

            // Filtra localmente a sprint do time
            var teamActiveSprint = activeSprints.FirstOrDefault(s =>
                s.ParticipatingMemberIds != null &&
                teamMemberIdStrings.Any(idStr => s.ParticipatingMemberIds.Contains(idStr)));

            return teamActiveSprint;
        }

        private async Task<HomeMetricsViewModel> GetSprintSummary(Colaborador loggedUser)
        {
            var teamActiveSprint = await GetActiveTeamSprint(loggedUser);
            var summary = new HomeMetricsViewModel { IsSprintActive = false };

            if (teamActiveSprint == null)
            {
                return summary;
            }

            // --- Cálculo de Progresso ---
            var today = DateTime.Today;
            var start = teamActiveSprint.StartDate.Date;
            var end = teamActiveSprint.EndDate.Date;

            double totalDays = (end - start).TotalDays + 1;
            double passedDays = (today - start).TotalDays + 1;

            double progress = 0;
            if (totalDays > 0 && today >= start)
            {
                progress = Math.Min(100, (passedDays / totalDays) * 100);
            }
 

            var impactNotifications = new List<string>();

            if (progress > 50 && progress < 80)
            {
                impactNotifications.Add("Sprint is progressing well. Current phase: Testing.");
            }
            if (passedDays > totalDays * 0.8)
            {
                impactNotifications.Add($"Heads up! Sprint ending soon ({end:MMM dd}). Final review required.");
            }

         

            // ---  Montar o Summary ---
            summary.IsSprintActive = true;
            summary.SprintName = teamActiveSprint.Name;
            summary.SprintDuration = $"{start:MMM dd} - {end:MMM dd}";
            summary.SprintProgressPercent = Math.Round(progress, 1);
            summary.SprintImpactNotifications = impactNotifications.Take(2).ToList();

            return summary;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Logout", "Login");

            var colaboradorLogado = await _context.Colaboradores
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (colaboradorLogado == null) return RedirectToAction("Logout", "Login");

            bool isRh = colaboradorLogado.Position.Contains("HR");
            bool isManager = colaboradorLogado.Position == "Project Manager";

           
            var sprintSummary = await GetSprintSummary(colaboradorLogado);

            //  RH
            if (isRh)
            {
                int pendingHrCount = await _requestService.GetPendingHRRequestsCountAsync();
                int totalEmployees = await _context.Colaboradores.CountAsync(c => c.Email != "recursoshumanos@flexcap.com");
                int activeSectorsCount = await _context.Colaboradores
                    .Where(c => c.Department != "RH")
                    .Select(c => c.Department)
                    .Distinct()
                    .CountAsync();

                var latestPendingRequests = await _context.Requests
                    .Include(r => r.Colaborador).Where(r => r.Status == "Waiting For HR").OrderByDescending(r => r.CreationDate).Take(4)
                    .Select(r => new PendingRequestListItemViewModel
                    {
                        RequestId = r.Id,
                        Subject = r.Subject,
                        CurrentStatus = r.Status,
                        SubmissionDate = r.CreationDate,
                        CollaboratorName = r.Colaborador.FullName,
                        Position = r.Colaborador.Position,
                        Department = r.Colaborador.Department,
                        TypeName = r.Colaborador.Department
                    })
                    .ToListAsync();

                var viewModel = new HomeMetricsViewModel
                {
                    TotalPendingHR = pendingHrCount,
                    TotalEmployees = totalEmployees,
                    ActiveSectorsCount = activeSectorsCount,
                    LatestPendingRequests = latestPendingRequests
                };

                ViewData["TotalColaboradores"] = totalEmployees.ToString();
                ViewData["TotalSetores"] = activeSectorsCount.ToString();

                return View("Rh", viewModel);
            }

            //  Manager
            else if (isManager)
            {
                var managerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int.TryParse(managerIdString, out int managerId);

                var teamName = colaboradorLogado.TeamName ?? "Sem Time";
                var normalizedTeam = colaboradorLogado.TeamName?.Trim().ToLower();

                var activeMembers = await _context.Colaboradores
                    .CountAsync(c => c.TeamName != null &&
                                    c.TeamName.Trim().ToLower() == normalizedTeam &&
                                    c.Status == "Active");

                int activeSprintCount = sprintSummary.IsSprintActive ? 1 : 0;

                var pendingRequests = await _context.Requests
                    .Include(r => r.Colaborador).Include(r => r.RequestType)
                    .Where(r => r.Status == "Waiting For Manager" && r.CurrentManagerId == managerId)
                    .OrderByDescending(r => r.CreationDate).Take(5)
                    .Select(r => new PendingRequestListItemViewModel
                    {
                        RequestId = r.Id,
                        Subject = r.Subject,
                        SubmissionDate = r.CreationDate,
                        CurrentStatus = r.Status,
                        CollaboratorName = r.Colaborador.FullName,
                        Department = r.Colaborador.Department,
                        Position = r.Colaborador.Position,
                        PhotoUrl = r.Colaborador.PhotoUrl,
                        TypeName = r.RequestType.Name
                    })
                    .ToListAsync();

                var viewModel = new HomeMetricsViewModel
                {
                    FirstName = colaboradorLogado.FullName?.Split(' ')?.FirstOrDefault() ?? "Usuário",
                    UserTeam = teamName,
                    ActiveMembers = activeMembers,
                    ActiveSprintCount = activeSprintCount,
                    PendingManagerRequests = pendingRequests,

                    // ADICIONA OS DADOS DA SPRINT
                    IsSprintActive = sprintSummary.IsSprintActive,
                    SprintName = sprintSummary.SprintName,
                    SprintDuration = sprintSummary.SprintDuration,
                    SprintProgressPercent = sprintSummary.SprintProgressPercent,
                    SprintImpactNotifications = sprintSummary.SprintImpactNotifications
                };

                return View("~/Views/Home/Manager.cshtml", viewModel);
            }

            else
            {
                var collaboratorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int.TryParse(collaboratorIdString, out int collaboratorId);

                var nomeDoTime = colaboradorLogado.TeamName ?? "Sem Time";
                var primeiroNome = colaboradorLogado.FullName?.Split(' ')[0] ?? "Colaborador";

                var membrosAtivos = await _context.Colaboradores
                    .CountAsync(c => c.TeamName == nomeDoTime && c.Status == "Active");
                var totalMembrosTime = await _context.Colaboradores
                    .CountAsync(c => c.TeamName == nomeDoTime);

                var recentRequests = await _context.Requests
                    .Include(r => r.RequestType)
                    .Where(r => r.CollaboratorId == collaboratorId)
                    .OrderByDescending(r => r.CreationDate).Take(3)
                    .Select(r => new PendingRequestListItemViewModel
                    {
                        CurrentStatus = r.Status,
                        SubmissionDate = r.CreationDate,
                        TypeName = r.RequestType.Name,
                        RequestId = r.Id
                    })
                    .ToListAsync();

                var viewModel = new HomeMetricsViewModel
                {
                    FirstName = primeiroNome,
                    UserTeam = nomeDoTime,
                    ActiveMembers = membrosAtivos,
                    TotalMembers = totalMembrosTime,
                    ColaboratorHistory = recentRequests,

                    // ADICIONA OS DADOS DA SPRINT
                    IsSprintActive = sprintSummary.IsSprintActive,
                    SprintName = sprintSummary.SprintName,
                    SprintDuration = sprintSummary.SprintDuration,
                    SprintProgressPercent = sprintSummary.SprintProgressPercent,
                    SprintImpactNotifications = sprintSummary.SprintImpactNotifications
                };

                return View("Colaborador", viewModel);
            }
        }
    }
}