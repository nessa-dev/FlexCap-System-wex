using FlexCap.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using FlexCap.Web.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; 

namespace FlexCap.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var fullNameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                if (!string.IsNullOrEmpty(fullNameClaim))
                {
                    ViewData["FirstName"] = fullNameClaim.Split(' ')[0];
                }

                return View();
            }

            ViewData["FirstName"] = "Visitante";
            return View();
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

            // Se o usuário não estiver logado, redireciona (embora o [Authorize] já proteja)
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

                // 1. CÁLCULO DE MÉTRICAS OTIMIZADO (ASYNC)
                int membrosAtivos = await _context.Colaboradores
                                                    .CountAsync(c => c.TeamName == nomeDoTime && c.Status == "Ativo");

                int totalMembrosTime = await _context.Colaboradores
                                                    .CountAsync(c => c.TeamName == nomeDoTime);

                // 2. BUSCA DO TIME (MANTIDA, AGORA ASYNC E ORDENADA)
                // Esta busca só é necessária se a View for a lista de cards
                var colaboradoresDoTime = await _context.Colaboradores
                    .AsNoTracking()
                    .Where(c => c.TeamName == nomeDoTime)
                    .OrderByDescending(c => c.Position == "Project Manager") // Ordena por Manager
                    .ThenBy(c => c.FullName)
                    .ToListAsync();


                // DEFINE VIEW DATA
                ViewData["FirstName"] = primeiroNome;
                ViewData["UserTeam"] = nomeDoTime;
                ViewData["ActiveMembers"] = membrosAtivos;
                ViewData["TotalMembers"] = totalMembrosTime;

                // Se HomeManager for um Dashboard, retorne apenas a View (sem Model)
                // Se HomeManager for a lista de cards, retorne com o Model
                return View("HomeManager", colaboradoresDoTime);
            }

            // Retorno de fallback (Se o usuário estiver autenticado, mas o registro não estiver no DB)
            ViewData["FirstName"] = "Usuário";
            ViewData["UserTeam"] = "Sem Time";
            ViewData["ActiveMembers"] = 0;
            return View("HomeManager", new List<Colaborador>());
        }




    }
}