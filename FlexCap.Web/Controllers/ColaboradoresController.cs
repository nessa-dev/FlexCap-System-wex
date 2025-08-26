using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
namespace FlexCap.Web.Controllers
{
    public class ColaboradoresController : Controller
    {
       
        public IActionResult Colaborador()
        {
            ViewData["Title"] = "Colaboradores da Equipe";
            ViewData["Profile"] = "Colaborador";
            return View();
        }

        public IActionResult Manager()
        {
            ViewData["Title"] = "Gestão de Colaboradores";
            ViewData["Profile"] = "Manager";
            return View();
        }

        public IActionResult Rh()
        {
            ViewData["Title"] = "Gestão de Colaboradores (RH)";
            ViewData["Profile"] = "Rh";
            return View();
        }
    }
}