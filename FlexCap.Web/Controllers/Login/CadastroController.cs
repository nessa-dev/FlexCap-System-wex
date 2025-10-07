using BCrypt.Net;
using FlexCap.Web.Data;
using FlexCap.Web.Models;
using FlexCap.Web.Models.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                bool emailExists = _context.Colaboradores
                                            .Any(c => c.Email == model.EmailAddress);

                if (emailExists)
                {
                    ModelState.AddModelError("EmailAddress", "The provided email is already registered.");
                }
                else
                {
                    var novoColaborador = new Colaborador
                    {
                        FullName = model.FullName!,
                        Email = model.EmailAddress!,
                        Position = model.Position ?? "", 
                        Department = model.Department ?? "", 
                        TeamName = model.Team ?? "", 
                        Country = model.CountryOfOperation ?? "", 
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password!)
                    };
                    _context.Colaboradores.Add(novoColaborador);
                    _context.SaveChanges();
                    return RedirectToAction("Index", "Login");
                }
            }
            return View(model);
        }

    }
}