using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FlexCap.Web.Controllers
{
    public class RequestsController : Controller
    {
        
        public IActionResult Manager()
        {
            ViewData["Title"] = "Solicitações da Equipe";
            ViewData["Profile"] = "Manager";
            return View();
        }

        public IActionResult Rh()
        {
            ViewData["Title"] = "Gestão de Solicitações (RH)";
            ViewData["Profile"] = "Rh";
            return View();
        }

        public IActionResult Colaborador()
        {
            ViewData["Title"] = "Minhas Solicitações";
            ViewData["Profile"] = "Colaborador";
            return View();
        }
    }
}