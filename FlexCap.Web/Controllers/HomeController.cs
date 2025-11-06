using FlexCap.Web.Data;
using FlexCap.Web.Models;
using FlexCap.Web.Services;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FlexCap.Web.Models.Requests;

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


            if (isRh)
            {

                int pendingHrCount = await _requestService.GetPendingHRRequestsCountAsync();
                int totalEmployees = await _context.Colaboradores.CountAsync(c => c.Email != "recursoshumanos@flexcap.com");
                int activeSectorsCount = await _context.Colaboradores.Where(c => c.Department != "RH")
                                                .Select(c => c.Department)
                                                .Distinct()
                                                .CountAsync();


                var latestPendingRequests = await _context.Requests
                    .Include(r => r.Colaborador) 
                    .Where(r => r.Status == "Aguardando RH")
                    .OrderByDescending(r => r.CreationDate)
                    .Take(5) 
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

            else if (isManager)
            {
                return RedirectToAction("Manager");
            }
            else
            {
                return RedirectToAction("Colaborador");
            }
        }











        [Authorize]
        public async Task<IActionResult> Colaborador()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            Colaborador colaboradorLogado = null;
            int collaboratorId = -1;

            if (!string.IsNullOrEmpty(userEmail))
            {
                colaboradorLogado = await _context.Colaboradores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Email == userEmail);

                var collaboratorIdString = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (int.TryParse(collaboratorIdString, out int id))
                {
                    collaboratorId = id;
                }
            }

            var viewModel = new HomeMetricsViewModel(); // Usa a nova ViewModel

            if (colaboradorLogado != null && collaboratorId != -1)
            {
                string nomeDoTime = colaboradorLogado.TeamName ?? "Sem Time";
                string primeiroNome = colaboradorLogado.FullName?.Split(' ')[0] ?? "Colaborador";

                // Busca Métricas do Time
                int membrosAtivos = await _context.Colaboradores.CountAsync(c => c.TeamName == nomeDoTime && c.Status == "Ativo");
                int totalMembrosTime = await _context.Colaboradores.CountAsync(c => c.TeamName == nomeDoTime);

                // 🛑 BUSCA DO HISTÓRICO: Puxando as 3 mais recentes
                var recentRequests = await _context.Requests
                    .Include(r => r.RequestType) // Incluir o Tipo para o nome
                    .Where(r => r.CollaboratorId == collaboratorId)
                    .OrderByDescending(r => r.CreationDate)
                    .Take(3) // 🛑 APENAS AS TRÊS MAIS RECENTES
                    .Select(r => new PendingRequestListItemViewModel
                    {
                        CurrentStatus = r.Status,
                        SubmissionDate = r.CreationDate,
                        TypeName = r.RequestType.Name,
                        RequestId = r.Id // Para futuros detalhes
                    })
                    .ToListAsync();

                // Preenche o ViewModel (Substituindo o ViewData)
                viewModel.FirstName = primeiroNome;
                viewModel.UserTeam = nomeDoTime;
                viewModel.ActiveMembers = membrosAtivos;
                viewModel.TotalMembers = totalMembrosTime;
                viewModel.ColaboratorHistory = recentRequests; 

            }
            // Retorna a View com a ViewModel completa
            return View("~/Views/Home/colaborador.cshtml", viewModel);
        }







        [Authorize]
        public async Task<IActionResult> Manager()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Logout", "Login");

            var manager = await _context.Colaboradores
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (manager == null)
                return RedirectToAction("Logout", "Login");

            // ✅ Pegando o time do gerente
            var teamName = manager.TeamName ?? "Sem Time";

            // ✅ Contando membros ativos do time
            var activeMembers = await _context.Colaboradores
                .CountAsync(c => c.TeamName == teamName && c.Status == "Ativo");

            // ✅ Pegando as requisições pendentes do time
            var pendingRequests = await _context.Requests
                .Include(r => r.Colaborador)
            .Where(r => r.Status == "Waiting For Manager" && r.Colaborador.TeamName == teamName)
                .OrderByDescending(r => r.CreationDate)
                .Take(5)
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
                // ✅ Usa apenas o primeiro nome extraído de FullName
                FirstName = manager.FullName?.Split(' ')?.FirstOrDefault() ?? "Usuário",
                UserTeam = teamName,
                ActiveMembers = activeMembers,
                PendingManagerRequests = pendingRequests
            };

            return View("Manager", viewModel);
        }








    }
}