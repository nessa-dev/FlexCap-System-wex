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

        public async Task<IActionResult> Colaborador()
        {
            ViewData["Profile"] = "Colaborador";
            Colaborador colaboradorLogado = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                if (userEmail != null)
                {
                    colaboradorLogado = await _context.Colaboradores
                        .FirstOrDefaultAsync(c => c.Email == userEmail);
                }
            }

            if (colaboradorLogado != null)
            {
                string primeiroNome = colaboradorLogado.FullName?.Split(' ')[0] ?? "Colaborador";
                string nomeDoTime = colaboradorLogado.TeamName ?? "Time Não Informado";

                ViewData["FirstName"] = primeiroNome;
                ViewData["UserTeam"] = nomeDoTime;

                ViewData["ActiveMembers"] = nomeDoTime != "Time Não Informado"
                    ? _context.Colaboradores.Count(c => c.TeamName == nomeDoTime && c.Status == "Ativo")
                    : 1;

                
                var colaboradoresDoTime = await _context.Colaboradores
                    .Where(c => c.TeamName == nomeDoTime)
                    .ToListAsync();

                return View(colaboradoresDoTime);
            }

            
            ViewData["FirstName"] = "Usuário";
            ViewData["UserTeam"] = "Sem Time";
            ViewData["ActiveMembers"] = 0;
            return View(new List<Colaborador>());
        }

       
        public async Task<IActionResult> Manager()
        {
            ViewData["Profile"] = "Manager";
            Colaborador colaboradorLogado = null;

            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (!string.IsNullOrEmpty(userEmail))
            {
                colaboradorLogado = await _context.Colaboradores.FirstOrDefaultAsync(c => c.Email == userEmail);
            }

            if (colaboradorLogado != null)
            {
                string primeiroNome = colaboradorLogado.FullName?.Split(' ')[0] ?? "Manager";
                string nomeDoTime = colaboradorLogado.TeamName ?? "Sem Time";

                ViewData["FirstName"] = primeiroNome;
                ViewData["UserTeam"] = nomeDoTime;

                int membrosAtivos = _context.Colaboradores.Count(c => c.TeamName == nomeDoTime && c.Status == "Ativo");
                ViewData["ActiveMembers"] = membrosAtivos;

                var colaboradoresDoTime = await _context.Colaboradores
                    .Where(c => c.TeamName == nomeDoTime)
                    .ToListAsync();

                return View(colaboradoresDoTime);
            }

            ViewData["FirstName"] = "Usuário";
            ViewData["UserTeam"] = "Sem Time";
            ViewData["ActiveMembers"] = 0;
            return View(new List<Colaborador>());
        }

        [Authorize(Roles = "HR Analyst, HR Consultant, HR Manager")]
        public IActionResult Rh()
        {
            ViewData["Profile"] = "Rh";

            var colaboradores = _context.Colaboradores.ToList();

            ViewData["TotalColaboradores"] = colaboradores.Count;
            ViewData["TotalSetores"] = colaboradores.Select(c => c.Department).Distinct().Count();

            return View(colaboradores);
        }
    }
}