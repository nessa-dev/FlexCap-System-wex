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
            // Verifica se o modelo (campos) é válido
            if (ModelState.IsValid)
            {
                // 1. TENTA BUSCAR O COLABORADOR PELO E-MAIL
                var colaborador = await _context.Colaboradores
                    .FirstOrDefaultAsync(c => c.Email == model.Email);

                // --- VERIFICAÇÃO EXCLUSIVA PARA CONTA DE SERVIÇO RH ---
                // Se o colaborador existir E a senha estiver correta, verifica o e-mail de serviço
                if (colaborador != null && BCrypt.Net.BCrypt.Verify(model.Senha, colaborador.PasswordHash))
                {
                    // E-MAIL DE SERVIÇO DO RH (SUPER USUÁRIO)
                    if (colaborador.Email.Equals("recursoshumanos@flexcap.com", StringComparison.OrdinalIgnoreCase))
                    {
                        // Garante que o perfil RH seja salvo nas Claims para acesso total
                        var claimsRh = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, ToTitleCase(colaborador.FullName)),
                    new Claim(ClaimTypes.Email, colaborador.Email),
                    new Claim("TeamName", colaborador.TeamName),
                    new Claim(ClaimTypes.Role, "HR Manager") // Posição Elevada
                };

                        var identityRh = new ClaimsIdentity(claimsRh, "login");
                        var principalRh = new ClaimsPrincipal(identityRh);
                        await HttpContext.SignInAsync(principalRh);

                        return RedirectToAction("Rh", "Home"); // REDIRECIONAMENTO IMEDIATO
                    }


                    // --- LOGIN PADRÃO (Se não for a conta de serviço) ---

                    // Formata e define as Claims
                    string formattedName = ToTitleCase(colaborador.FullName);

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, formattedName),
                new Claim(ClaimTypes.Email, colaborador.Email),
                new Claim("TeamName", colaborador.TeamName),
                new Claim(ClaimTypes.Role, colaborador.Position)
            };

                    var identity = new ClaimsIdentity(claims, "login");
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(principal);

                    // Redirecionamento baseado na Posição
                    if (colaborador.Position == "Project Manager")
                    {
                        return RedirectToAction("Manager", "Home");
                    }
                    else if (colaborador.Position == "HR Analyst" || colaborador.Position == "HR Consultant")
                    {
                        return RedirectToAction("Rh", "Home"); // Redirecionamento RH
                    }
                    else
                    {
                        return RedirectToAction("Colaborador", "Home"); // Colaborador Padrão
                    }
                }

                // --- LOGIN FALHOU ---
                ModelState.AddModelError("", "Invalid email or password.");
            }

            // Retorna à View (exibindo erros)
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