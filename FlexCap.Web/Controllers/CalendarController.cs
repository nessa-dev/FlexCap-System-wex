using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FlexCap.Web.Controllers
{
    public class CalendarController : Controller
    {
        // Ação para o calendário do RH
        // [Authorize(Roles = "RH")]
        public IActionResult Rh()
        {
            ViewData["Title"] = "Calendário do RH";
            return View();
        }

        // Ação para o calendário do Manager
        // [Authorize(Roles = "Manager")]
        public IActionResult Manager()
        {
            ViewData["Title"] = "Calendário do Manager";
            return View();
        }

        // Ação para o calendário do Colaborador Geral
        // [Authorize(Roles = "ColaboradorGeral")]
        public IActionResult Colaborador()
        {
            ViewData["Title"] = "Calendário do Colaborador";
            return View();
        }
    }
}