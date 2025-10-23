// FlexCap.Web.Controllers/RegistroController.cs

using BCrypt.Net;
using FlexCap.Web.Data;
using FlexCap.Web.Models; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; 
using System.Collections.Generic; 
using System.Threading.Tasks;
using System; 
using System.IO;

namespace FlexCap.Web.Controllers
{
    [Authorize(Roles = "HR Manager, HR Analyst, HR Consultant")]
    public class RegistroController : Controller
    {
        private readonly AppDbContext _context;

        public RegistroController(AppDbContext context)
        {
            _context = context;
        }





        private Colaborador CriarNovoColaborador(ColaboradorViewModel model)
        {
            string finalPosition = model.Position!;
            if (model.IsManager)
            {
                finalPosition = "Project Manager";
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Senha!);

            string photoUrl = model.PhotoUrl ?? "https://randomuser.me/api/portraits/lego/1.jpg";

            return new Colaborador
            {
                FullName = model.FullName!,
                Email = model.Email!,
                PasswordHash = hashedPassword,
                Position = finalPosition,
                Department = model.Department,
                TeamName = model.TeamName,
                Country = model.Country,
                PhotoUrl = photoUrl,
                Status = "Ativo"
            };
        }

        private void PopulateViewBags()
        {
            ViewBag.Departments = new SelectList(new[] { "Benefits", "Mobility", "Corporate Payments", "RH" });
            ViewBag.Teams = new SelectList(new[] { "Titans", "Code Warriors", "Bug Busters", "Pixel Pioneers", "Cloud Crusaders", "Dev Dynamos", "RH Operations" });
            ViewBag.Countries = new SelectList(new[] { "Brazil", "United States", "Italy", "Japan", "Canada" });
            ViewBag.Positions = new SelectList(new[] { "Dev Sênior", "Dev Pleno", "QA Sênior", "Project Manager", "HR Analyst", "Intern", "Designer UX/UI", "Sales Analyst", "Administrative Assistant", "QA Tester", "Marketing Manager", "HR Consultant", "Dev Back-end", "UX Strategist", "Cloud Engineer", "Full Stack Dev" });

            ViewBag.InactivityReasons = new SelectList(new[] {
        "Medical Leave",
        "Personal License",
        "Vacation",
        "Day Off",
        "Maternity Leave",
        "Suspension",
        "Other"
    });
        }




        // --- (GET - Exibir Formulário) ---
        [HttpGet]
        public IActionResult NovoColaborador()
        {
            ViewData["Title"] = "Cadastrar Novo Colaborador";
            ViewData["Profile"] = "Rh";
            PopulateViewBags();

            ViewBag.Departments = new SelectList(new[] { "Benefits", "Mobility", "Corporate Payments", "RH" });
            ViewBag.Teams = new SelectList(new[] { "Titans", "Code Warriors", "Bug Busters", "Pixel Pioneers", "Cloud Crusaders", "Dev Dynamos", "RH Operations" });
            ViewBag.Countries = new SelectList(new[] { "Brazil", "United States", "Italy", "Japan", "Canada" });
            ViewBag.Positions = new SelectList(new[] { "Dev Sênior", "Dev Pleno", "QA Sênior", "Project Manager", "HR Analyst", "Intern", "Designer UX/UI", "Sales Analyst", "Administrative Assistant", "QA Tester", "Marketing Manager", "HR Consultant", "Dev Back-end", "UX Strategist", "Cloud Engineer", "Full Stack Dev" });

            return View("CadastrarUsuario", new ColaboradorViewModel() { Status = "Ativo" });
        }



        // --- (POST - Salvar Dados) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovoColaborador(ColaboradorViewModel model)
        {
            if (model.Senha == null)
            {
                ModelState.AddModelError("Senha", "A senha é obrigatória.");
            }

            if (ModelState.IsValid)
            {
                string senhaOriginal = model.Senha!;

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(senhaOriginal);
                string finalPosition = model.IsManager ? "Project Manager" : model.Position!;

                string photoUrl = "https://randomuser.me/api/portraits/lego/1.jpg";
                if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    Directory.CreateDirectory(uploadsFolder);
                    string fileName = Guid.NewGuid() + Path.GetExtension(model.PhotoFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.PhotoFile.CopyToAsync(stream);
                    }
                    photoUrl = "/uploads/" + fileName;
                }
                else if (!string.IsNullOrEmpty(model.PhotoUrl))
                {
                    photoUrl = model.PhotoUrl;
                }

                var novoColaborador = new Colaborador
                {
                    FullName = model.FullName!,
                    Email = model.Email!,
                    PasswordHash = hashedPassword, 
                    Position = finalPosition,
                    Department = model.Department,
                    TeamName = model.TeamName,
                    Country = model.Country,
                    PhotoUrl = photoUrl,
                    Status = model.Status ?? "Ativo"
                };

                _context.Colaboradores.Add(novoColaborador);
                await _context.SaveChangesAsync();

                TempData["CadastroSucesso"] = true;
                TempData["NovoColaboradorNome"] = model.FullName;
                TempData["NovoColaboradorEmail"] = model.Email;
                TempData["NovoColaboradorSenha"] = senhaOriginal; 

                return RedirectToAction("Listar", "Colaboradores");
            }

            PopulateViewBags();
            return View("CadastrarUsuario", model);
        }




        // --- MÉTODOS PARA A NOVA TABELA (TabelaTeste) ---

        [Authorize(Roles = "HR Manager, HR Analyst, HR Consultant")]
        [HttpGet]
        public async Task<IActionResult> ListarDados()
        {
            ViewData["Title"] = "Listagem de Dados de Teste";
            ViewData["Profile"] = "Rh";

            var dados = await _context.DadosDeTeste.ToListAsync();

            return View(dados);
        }


        [Authorize(Roles = "HR Manager, HR Analyst, HR Consultant")]
        [HttpGet]
        public IActionResult CriarDado()
        {
            ViewData["Title"] = "Criar Novo Dado de Teste";
            ViewData["Profile"] = "Rh";
            return View(); 
        }


        [Authorize(Roles = "HR Manager, HR Analyst, HR Consultant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarDado([Bind("NomeDoDado,Quantidade")] TabelaTeste dado)
        {
            if (ModelState.IsValid)
            {
                dado.DataCriacao = DateTime.Now; 
                _context.DadosDeTeste.Add(dado);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ListarDados));
            }

            return View(dado);
        }


    }
}