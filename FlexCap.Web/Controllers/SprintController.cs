using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FlexCap.Web.Controllers
{
    public class SprintController : Controller
    {
       
        public IActionResult Manager()
        {
            ViewData["Title"] = "Sprint e Gantt do Manager";
            ViewData["Profile"] = "Manager";
            return View();
        }

        public IActionResult Rh()
        {
            ViewData["Title"] = "Sprint e Gantt do RH";
            ViewData["Profile"] = "Rh";
            return View();
        }

        public IActionResult Colaborador()
        {
            ViewData["Title"] = "Minhas Sprints";
            ViewData["Profile"] = "Colaborador";
            return View();
        }
    }
}