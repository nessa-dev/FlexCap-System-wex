using FlexCap.Web.Data;
using FlexCap.Web.Models;
using FlexCap.Web.Models.Account;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net; 

namespace FlexCap.Web.Controllers
{
    public class CadastroController : Controller
    {
        private readonly AppDbContext _context;

        public CadastroController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Cadastro()
        {
            return View(new CadastroModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cadastro(CadastroModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = new Usuario
                {
                    FullName = model.FullName!,
                    EmailAddress = model.EmailAddress!,
                    Position = model.Position!,
                    Department = model.Department!,
                    Team = model.Team,
                    CountryOfOperation = model.CountryOfOperation!,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password!)
                };

                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                return RedirectToAction("Index", "Login");
            }

            return View(model);
        }
    }
}
