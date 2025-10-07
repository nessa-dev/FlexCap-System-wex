using FlexCap.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using FlexCap.Web.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic; // Necessário para List<Colaborador>()

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

        // Action de Index: Deve ser simples e focar em redirecionamento ou saudação básica
        public async Task<IActionResult> Index()
        {
            // O nome completo (formatado em Title Case) é obtido via ClaimTypes.Name do LoginController
            var fullNameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (!string.IsNullOrEmpty(fullNameClaim))
            {
                // Extrai apenas o primeiro nome para a saudação
                ViewData["FirstName"] = fullNameClaim.Split(' ')[0];

                // Se o usuário estiver autenticado, você pode redirecionar para a Home/Colaborador
                // return RedirectToAction("Colaborador"); 
            }
            else
            {
                ViewData["FirstName"] = "Visitor"; // Traduzido para Inglês
            }

            return View(); // Retorna a View Index com a saudação
        }

        // Action Colaborador: Lista apenas os membros do time do usuário logado
        public async Task<IActionResult> Colaborador()
        {
            // Tenta obter o nome do time via Claim (foi salvo no LoginController)
            var userTeamName = User.Claims.FirstOrDefault(c => c.Type == "TeamName")?.Value;
            var fullName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            // Se a Claim do time não foi encontrada (ou está vazia)
            if (string.IsNullOrEmpty(userTeamName))
            {
                // Retorna a View com uma lista vazia ou exibe uma mensagem de erro
                ViewData["UserTeam"] = "No Team Assigned"; // Traduzido
                ViewData["FirstName"] = fullName?.Split(' ')[0] ?? "Colleague";
                ViewData["CurrentUserName"] = fullName;

                return View(new List<Colaborador>());
            }

            // --- LÓGICA DE FILTRAGEM POR TIME ---
            var colaboradoresDoTime = await _context.Colaboradores
                                            .Where(c => c.TeamName == userTeamName)
                                            .OrderBy(c => c.FullName)
                                            .ToListAsync();

            // Cálculos para o Overview Card
            var activeMembers = colaboradoresDoTime.Count(c => c.Status == "Ativo");

            // Preparação dos dados para a View
            ViewData["FirstName"] = fullName?.Split(' ')[0];
            ViewData["CurrentUserName"] = fullName; // Usado na View para marcar o próprio usuário
            ViewData["UserTeam"] = userTeamName;
            ViewData["ActiveMembers"] = activeMembers;


            return View(colaboradoresDoTime); // Envia apenas os membros do time para a View
        }

        // Action Manager: Já faz a filtragem, apenas ajustada para Claims e Async
        public async Task<IActionResult> Manager()
        {
            ViewData["Profile"] = "Manager";

            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (!string.IsNullOrEmpty(userEmail))
            {
                var colaboradorLogado = await _context.Colaboradores.FirstOrDefaultAsync(c => c.Email == userEmail);

                if (colaboradorLogado != null)
                {
                    string nomeDoTime = colaboradorLogado.TeamName;

                    ViewData["FirstName"] = colaboradorLogado.FullName?.Split(' ')[0] ?? "Manager";
                    ViewData["UserTeam"] = nomeDoTime;

                    var colaboradoresDoTime = await _context.Colaboradores
                                                            .Where(c => c.TeamName == nomeDoTime)
                                                            .ToListAsync();

                    int membrosAtivos = colaboradoresDoTime.Count(c => c.Status == "Ativo");
                    ViewData["ActiveMembers"] = membrosAtivos;

                    return View(colaboradoresDoTime);
                }
            }

            // Retorna lista vazia e dados padrão se não puder carregar o perfil
            ViewData["FirstName"] = "Manager";
            ViewData["UserTeam"] = "No Team Assigned";
            ViewData["ActiveMembers"] = 0;
            return View(new List<Colaborador>());
        }

        public IActionResult Rh()
        {
            // Manter como estava, listando todos para o RH
            ViewData["Profile"] = "HR";
            var colaboradores = _context.Colaboradores.ToList();

            ViewData["TotalColaboradores"] = colaboradores.Count;
            ViewData["TotalSetores"] = colaboradores.Select(c => c.Department).Distinct().Count();

            return View(colaboradores);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
