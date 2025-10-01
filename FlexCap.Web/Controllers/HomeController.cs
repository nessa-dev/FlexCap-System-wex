using FlexCap.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using FlexCap.Web.Data;
using System.Linq;
using System.Security.Claims; 

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

        public async Task<IActionResult> Colaborador()
        {
            ViewData["Profile"] = "Colaborador";
            Colaborador colaboradorLogado = null;

            int? userId = TempData["UserId"] as int?;
            if (userId.HasValue)
            {
                colaboradorLogado = await _context.Colaboradores.FindAsync(userId.Value);
            }

            if (colaboradorLogado == null && User?.Identity?.IsAuthenticated == true)
            {
                var userEmail = User.Identity.Name;
                colaboradorLogado = await _context.Colaboradores
                    .FirstOrDefaultAsync(c => c.Email == userEmail);
            }

            if (colaboradorLogado != null)
            {
                string primeiroNome = colaboradorLogado.NomeCompleto?.Split(' ')[0] ?? "Usuário";

                string nomeDoTime = string.IsNullOrEmpty(colaboradorLogado.Time)
                    ? "Time Não Informado"
                    : colaboradorLogado.Time;

                ViewData["FirstName"] = primeiroNome;
                ViewData["UserTeam"] = nomeDoTime;

                ViewData["ActiveMembers"] = nomeDoTime != "Time Não Informado"
                    ? _context.Colaboradores.Count(c => c.Time == nomeDoTime && c.Status == "Ativo")
                    : 1;
            }
            else
            {
                ViewData["FirstName"] = "Usuário";
                ViewData["UserTeam"] = "Sem Time";
                ViewData["ActiveMembers"] = 0;
            }

            return View(await _context.Colaboradores.ToListAsync());
        }





        public async Task<IActionResult> Manager()
        {
            ViewData["Profile"] = "Manager";

            int? userId = TempData["UserId"] as int?;
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