using FlexCap.Web.Data;
using FlexCap.Web.Models.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FlexCap.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _context.Colaboradores
                    .FirstOrDefaultAsync(c => c.Email == model.Email);

                if (usuario != null && BCrypt.Net.BCrypt.Verify(model.Senha, usuario.Senha))
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NomeCompleto) 
            };

                    var identity = new ClaimsIdentity(claims, "login");
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(principal);

                    TempData["UserId"] = usuario.Id;

                    return RedirectToAction("Colaborador", "Home");
                }

                ModelState.AddModelError("", "E-mail ou senha inválidos.");
            }

            return View(model);
        }


        public async Task<IActionResult> Entrar(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Home");
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.EmailAddress == email);

            if (usuario != null)
            {
                TempData["UserId"] = usuario.Id;
                TempData["UserProfile"] = usuario.Position;

                return RedirectToAction("Colaborador", "Home");
            }

            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Email == email);

            if (colaborador != null)
            {
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

            return RedirectToAction("Index", "Home");
        }
    }
}