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



                if (colaborador != null && BCrypt.Net.BCrypt.Verify(model.Senha, colaborador.PasswordHash))
                {

                    if (colaborador.Email.Equals("recursoshumanos@flexcap.com", StringComparison.OrdinalIgnoreCase))
                    {

                        var claimsRh = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, ToTitleCase(colaborador.FullName)),
                    new Claim(ClaimTypes.Email, colaborador.Email),
                    new Claim("TeamName", colaborador.TeamName),
                    new Claim(ClaimTypes.Role, "HR Manager") 

                };

                        var identityRh = new ClaimsIdentity(claimsRh, "login");
                        var principalRh = new ClaimsPrincipal(identityRh);
                        await HttpContext.SignInAsync(principalRh);

                        return RedirectToAction("Rh", "Home"); 

                    }





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

        // Logout method
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }

    }
}