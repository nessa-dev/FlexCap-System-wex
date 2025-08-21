using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FlexCap.Web.Controllers
{
    public class RequestsController : Controller
    {
        // Esta página será para o Manager ver as solicitações de sua equipe
        // [Authorize(Roles = "Manager")]
        public IActionResult Manager()
        {
            ViewData["Title"] = "Solicitações da Equipe";
            return View();
        }

        // Esta página será para o RH gerenciar todas as solicitações
        // [Authorize(Roles = "RH")]
        public IActionResult Rh()
        {
            ViewData["Title"] = "Gestão de Solicitações (RH)";
            return View();
        }

        // Esta página será para o Colaborador Geral ver suas próprias solicitações
        // [Authorize(Roles = "ColaboradorGeral")]
        public IActionResult Colaborador()
        {
            ViewData["Title"] = "Minhas Solicitações";
            return View();
        }
    }
}