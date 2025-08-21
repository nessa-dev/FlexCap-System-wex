using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Necessário para usar o [Authorize]

namespace FlexCap.Web.Controllers
{
    public class ColaboradoresController : Controller
    {
        // Esta página será para o Colaborador Geral visualizar sua equipe
        // [Authorize(Roles = "ColaboradorGeral")]
        public IActionResult Colaborador()
        {
            ViewData["Title"] = "Colaboradores da Equipe";
            return View();
        }

        // Esta página será para o Manager gerenciar os colaboradores da sua equipe
        // [Authorize(Roles = "Manager")]
        public IActionResult Manager()
        {
            ViewData["Title"] = "Gestão de Colaboradores";
            return View();
        }

        // Esta página será para o RH gerenciar todos os colaboradores
        // [Authorize(Roles = "RH")]
        public IActionResult Rh()
        {
            ViewData["Title"] = "Gestão de Colaboradores (RH)";
            return View();
        }
    }
}