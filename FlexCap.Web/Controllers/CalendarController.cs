using Microsoft.AspNetCore.Mvc;

namespace FlexCap.Web.Controllers
{
    public class CalendarController : Controller
    {
        public IActionResult Rh()
        {
            ViewData["Title"] = "Calendário do RH";
            ViewData["Profile"] = "Rh";
            return View();
        }
        public IActionResult Manager()
        {
            ViewData["Title"] = "Calendário do Manager";
            ViewData["Profile"] = "Manager";
            return View(); 
        }

        public IActionResult Colaborador()
        {
            ViewData["Title"] = "Calendário do Colaborador";
            ViewData["Profile"] = "Colaborador";
            return View(); 
        }
    }
}
