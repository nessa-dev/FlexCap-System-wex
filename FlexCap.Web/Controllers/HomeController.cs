using FlexCap.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using FlexCap.Web.Data;

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

        public IActionResult Rh()
        {
            ViewData["Profile"] = "Rh";
            return View();
        }

        public IActionResult Manager()
        {
            ViewData["Profile"] = "Manager";

            // Busque o colaborador no banco de dados
            var colaboradorLogado = _context.Colaboradores.FirstOrDefault(c => c.Email == "pedro.souza@flexcap.com");
            int membrosAtivos = 0;

            if (colaboradorLogado != null)
            {
                string primeiroNome = colaboradorLogado.NomeCompleto.Split(' ')[0];
                string nomeDoTime = colaboradorLogado.Time;
                ViewData["FirstName"] = primeiroNome;
                ViewData["UserTeam"] = colaboradorLogado.Time;
                membrosAtivos = _context.Colaboradores.Count(c => c.Time == nomeDoTime && c.Status == "Ativo");

            }
            else
            {
                ViewData["FirstName"] = "Usuário";
                ViewData["UserTeam"] = "Sem Time";

            }
            ViewData["ActiveMembers"] = membrosAtivos;

            return View();
        }

        public IActionResult Colaborador()
        {
            ViewData["Profile"] = "Colaborador";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}