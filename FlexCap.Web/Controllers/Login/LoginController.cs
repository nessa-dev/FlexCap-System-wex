using FlexCap.Web.Data;
using FlexCap.Web.Models;
using FlexCap.Web.Models.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;


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
                var colaborador = await _context.Colaboradores
                    .FirstOrDefaultAsync(c => c.Email == model.Email);

                // 1. Verifica se o colaborador existe e a senha está correta
                if (colaborador != null && BCrypt.Net.BCrypt.Verify(model.Senha, colaborador.PasswordHash))
                {
                    // Formata o nome para ter a primeira letra maiúscula (Ex: Vanessa Oliveira)
                    string formattedName = ToTitleCase(colaborador.FullName);

                    // --- SALVANDO AS CLAIMS CORRETAS ---
                    var claims = new List<Claim>
                    {
                        // Nome formatado para exibição
                        new Claim(ClaimTypes.Name, formattedName), 
                        // Email (CRUCIAL para buscar o colaborador em outras Actions)
                        new Claim(ClaimTypes.Email, colaborador.Email), 
                        // Nome do Time (CRUCIAL para a filtragem na Home)
                        new Claim("TeamName", colaborador.TeamName)
                    };
                    // ------------------------------------

                    var identity = new ClaimsIdentity(claims, "login");
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(principal);

                    // Redireciona com base no cargo (se necessário, mas o principal é a Home)
                    return RedirectToAction("Colaborador", "Home");
                }

                // E-mail ou senha inválidos
                ModelState.AddModelError("", "Invalid email or password.");
            }

            // Se o ModelState for inválido ou o login falhar
            return View("Index", model);
        }

        // Action auxiliar para formatar o nome
        private string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // Força a capitalização usando a cultura invariante (mais segura para nomes próprios)
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }





        public async Task<IActionResult> Entrar(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Home");
            }
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Email == email);

            if (colaborador != null)
            {
                TempData["UserId"] = colaborador.Id;

                TempData["UserProfile"] = colaborador.Position;

                if (colaborador.Position == "Project Manager") 
                {
                    return RedirectToAction("Manager", "Home");
                }
                else if (colaborador.Position == "HR Analyst" || colaborador.Position == "HR Consultant")
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
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }
    }
}