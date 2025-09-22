using FlexCap.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using FlexCap.Web.Data;
using System.Linq;

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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Manager()
        {
            ViewData["Profile"] = "Manager";

            // Lê o ID do TempData
            int? userId = TempData["UserId"] as int?;

            // Se o ID existir, busca o colaborador com base nele
            if (userId.HasValue)
            {
                var colaboradorLogado = await _context.Colaboradores.FindAsync(userId.Value);
                if (colaboradorLogado != null)
                {
                    string primeiroNome = colaboradorLogado.NomeCompleto.Split(' ')[0];
                    string nomeDoTime = colaboradorLogado.Time;

                    ViewData["FirstName"] = primeiroNome;
                    ViewData["UserTeam"] = nomeDoTime;

                    int membrosAtivos = _context.Colaboradores.Count(c => c.Time == nomeDoTime && c.Status == "Ativo");
                    ViewData["ActiveMembers"] = membrosAtivos;
                }
            }
            else
            {
                ViewData["FirstName"] = "Usuário";
                ViewData["UserTeam"] = "Sem Time";
                ViewData["ActiveMembers"] = 0;
            }

            return View();
        }

        public async Task<IActionResult> Colaborador()
        {
            ViewData["Profile"] = "Colaborador";

            // Lê o ID do TempData
            int? userId = TempData["UserId"] as int?;

            // Se o ID existir, busca o colaborador com base nele
            if (userId.HasValue)
            {
                var colaboradorLogado = await _context.Colaboradores.FindAsync(userId.Value);
                if (colaboradorLogado != null)
                {
                    string primeiroNome = colaboradorLogado.NomeCompleto.Split(' ')[0];
                    string nomeDoTime = colaboradorLogado.Time;

                    ViewData["FirstName"] = primeiroNome;
                    ViewData["UserTeam"] = nomeDoTime;

                    int membrosAtivos = _context.Colaboradores.Count(c => c.Time == nomeDoTime && c.Status == "Ativo");
                    ViewData["ActiveMembers"] = membrosAtivos;
                }
            }
            else
            {
                ViewData["FirstName"] = "Usuário";
                ViewData["UserTeam"] = "Sem Time";
                ViewData["ActiveMembers"] = 0;
            }

            return View();
        }

        public IActionResult Rh()
        {
            ViewData["Profile"] = "Rh";
            var colaboradores = _context.Colaboradores.ToList();

            ViewData["TotalColaboradores"] = colaboradores.Count;
            ViewData["TotalSetores"] = colaboradores.Select(c => c.Setor).Distinct().Count();

            return View(colaboradores);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}