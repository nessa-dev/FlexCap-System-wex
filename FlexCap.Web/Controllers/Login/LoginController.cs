using BCrypt.Net;
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
using Microsoft.AspNetCore.Authentication.Cookies; 

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

                if (colaborador != null && BCrypt.Net.BCrypt.Verify(model.Senha, colaborador.PasswordHash))
                {
                    string userIdString = colaborador.Id.ToString();
                    string formattedName = ToTitleCase(colaborador.FullName);

                    // Determina a Role correta para todos os colaboradores, incluindo o "HR Manager"
                    string role = colaborador.Email.Equals("recursoshumanos@flexcap.com", StringComparison.OrdinalIgnoreCase) ? "HR Manager" : colaborador.Position;

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, formattedName),
                new Claim(ClaimTypes.Email, colaborador.Email),
                new Claim("TeamName", colaborador.TeamName),
                new Claim(ClaimTypes.Role, role), // Usando a role definida acima
                new Claim(ClaimTypes.NameIdentifier, userIdString)
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(principal);

                    // Agora, o redirecionamento usa apenas a lógica de Position/Role:
                    if (role == "HR Manager" || colaborador.Position == "HR Analyst" || colaborador.Position == "HR Consultant")
                    {
                        // REDIRECIONAMENTO RH (DASHBOARD)
                        return RedirectToAction("Rh", "Colaboradores");
                    }
                    else if (colaborador.Position == "Project Manager")
                    {
                        // CORREÇÃO: REDIRECIONAMENTO MANAGER (DASHBOARD)
                        return RedirectToAction("Manager", "Colaboradores");
                    }
                    else
                    {
                        // CORREÇÃO: REDIRECIONAMENTO COLABORADOR GERAL (DASHBOARD)
                        return RedirectToAction("Colaborador", "Colaboradores");
                    }
                }

                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View("Index", model);
        }






















        private string ToTitleCase(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            TextInfo ti = CultureInfo.InvariantCulture.TextInfo;
            string titleCaseText = ti.ToTitleCase(text.ToLower());

            if (titleCaseText.Length > 0 && char.IsLower(titleCaseText[0]))
            {
                return char.ToUpper(titleCaseText[0]) + titleCaseText.Substring(1);
            }

            return titleCaseText;
        }









        










        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}