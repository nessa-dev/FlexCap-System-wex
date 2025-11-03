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
            ViewData["Profile"] = "Colaborador";
            Colaborador colaboradorLogado = null;

            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (!string.IsNullOrEmpty(userEmail))
            {
                colaboradorLogado = await _context.Colaboradores
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(c => c.Email == userEmail);
            }

            if (colaboradorLogado != null)
            {
                string primeiroNome = colaboradorLogado.FullName?.Split(' ')[0] ?? "Colaborador";
                string nomeDoTime = colaboradorLogado.TeamName ?? "Sem Time";

                int membrosAtivos = await _context.Colaboradores.CountAsync(c => c.TeamName == nomeDoTime && c.Status == "Ativo");
                int totalMembrosTime = await _context.Colaboradores.CountAsync(c => c.TeamName == nomeDoTime);

                ViewData["FirstName"] = primeiroNome;
                ViewData["UserTeam"] = nomeDoTime;
                ViewData["ActiveMembers"] = membrosAtivos;
                ViewData["TotalMembers"] = totalMembrosTime; 
                return View("~/Views/Home/colaborador.cshtml");
            }
            ViewData["FirstName"] = "Usuário";
            ViewData["UserTeam"] = "Sem Time";
            ViewData["ActiveMembers"] = 0;
            ViewData["TotalMembers"] = 0;
            return View("~/Views/Home/colaborador.cshtml");
        }







        [HttpGet]
        public async Task<IActionResult> Manager()
        {
            ViewData["Profile"] = "Manager";
            string userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Index", "Login");
            }

            var colaboradorLogado = await _context.Colaboradores
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (colaboradorLogado != null)
            {
                string primeiroNome = colaboradorLogado.FullName?.Split(' ')[0] ?? "Manager";
                string nomeDoTime = colaboradorLogado.TeamName ?? "Sem Time";

                int membrosAtivos = await _context.Colaboradores
                                                    .CountAsync(c => c.TeamName == nomeDoTime && c.Status == "Ativo");

                int totalMembrosTime = await _context.Colaboradores
                                                    .CountAsync(c => c.TeamName == nomeDoTime);

                var colaboradoresDoTime = await _context.Colaboradores
                    .AsNoTracking()
                    .Where(c => c.TeamName == nomeDoTime)
                    .OrderByDescending(c => c.Position == "Project Manager") 
                    .ThenBy(c => c.FullName)
                    .ToListAsync();

                ViewData["FirstName"] = primeiroNome;
                ViewData["UserTeam"] = nomeDoTime;
                ViewData["ActiveMembers"] = membrosAtivos;
                ViewData["TotalMembers"] = totalMembrosTime;
                return View("Manager", colaboradoresDoTime);
            }
            ViewData["FirstName"] = "Usuário";
            ViewData["UserTeam"] = "Sem Time";
            ViewData["ActiveMembers"] = 0;
            return View("Manager", new List<Colaborador>());
        }




    }
}