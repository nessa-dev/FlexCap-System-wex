using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FlexCap.Web.Controllers
{
    public class SprintController : Controller
    {
        // Esta página será para o Manager ver as sprints de sua equipe
        // [Authorize(Roles = "Manager")]
        public IActionResult Manager()
        {
            ViewData["Title"] = "Sprint e Gantt do Manager";
            return View();
        }

        // Esta página será para o RH ver o histórico de sprints
        // [Authorize(Roles = "RH")]
        public IActionResult Rh()
        {
            ViewData["Title"] = "Sprint e Gantt do RH";
            return View();
        }

        // Esta página será para o Colaborador Geral ver suas próprias sprints
        // [Authorize(Roles = "ColaboradorGeral")]
        public IActionResult Colaborador()
        {
            ViewData["Title"] = "Minhas Sprints";
            return View();
        }
    }
}