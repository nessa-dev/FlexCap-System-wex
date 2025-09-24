using Microsoft.AspNetCore.Mvc;
using FlexCap.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace FlexCap.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Entrar(string email)
        {
            var colaborador = await _context.Colaboradores
                                            .FirstOrDefaultAsync(c => c.Email == email);

            if (colaborador == null)
            {
                return RedirectToAction("Index", "Home");
            }

            TempData["UserId"] = colaborador.Id;
            TempData["UserProfile"] = colaborador.Cargo;

            if (colaborador.Cargo == "Gerente de Projeto")
            {
                return RedirectToAction("Manager", "Home");
            }
            else if (colaborador.Cargo == "Analista de RH" || colaborador.Cargo == "Consultora de RH")
            {
                return RedirectToAction("Rh", "Home");
            }
            else
            {
                return RedirectToAction("Colaborador", "Home");
            }
        }
    }
}