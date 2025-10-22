// FlexCap.Web.Controllers/RegistroController.cs

using BCrypt.Net;
using FlexCap.Web.Data;
using FlexCap.Web.Models; // GARANTE O RECONHECIMENTO DE ColaboradorViewModel e TabelaTeste
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // ESSENCIAL para ToListAsync
using System.Collections.Generic; // Para List<Colaborador>
using System.Threading.Tasks;
using System; // Para DateTime.Now
using System.IO;

namespace FlexCap.Web.Controllers
{
    // Apenas usuários de RH podem acessar as páginas de registro de funcionários
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
            // A lógica de Manager é aplicada aqui
            string finalPosition = model.Position!;
            if (model.IsManager)
            {
                finalPosition = "Project Manager";
            }

            // Criptografa a Senha
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Senha!);

            // Trata a PhotoUrl
            string photoUrl = model.PhotoUrl ?? "https://randomuser.me/api/portraits/lego/1.jpg";

            // Cria e retorna o objeto Colaborador final
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
            // ViewBags existentes
            ViewBag.Departments = new SelectList(new[] { "Benefits", "Mobility", "Corporate Payments", "RH" });
            ViewBag.Teams = new SelectList(new[] { "Titans", "Code Warriors", "Bug Busters", "Pixel Pioneers", "Cloud Crusaders", "Dev Dynamos", "RH Operations" });
            ViewBag.Countries = new SelectList(new[] { "Brazil", "United States", "Italy", "Japan", "Canada" });
            ViewBag.Positions = new SelectList(new[] { "Dev Sênior", "Dev Pleno", "QA Sênior", "Project Manager", "HR Analyst", "Intern", "Designer UX/UI", "Sales Analyst", "Administrative Assistant", "QA Tester", "Marketing Manager", "HR Consultant", "Dev Back-end", "UX Strategist", "Cloud Engineer", "Full Stack Dev" });

            // NOVO: ViewBag para os motivos de inatividade
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








        // --- ROTA: /Registro/NovoColaborador (GET - Exibir Formulário) ---
        [HttpGet]
        public IActionResult NovoColaborador()
        {
            ViewData["Title"] = "Cadastrar Novo Colaborador";
            ViewData["Profile"] = "Rh";

            // Reutilize as listas de opções para dropdowns
            ViewBag.Departments = new SelectList(new[] { "Benefits", "Mobility", "Corporate Payments", "RH" });
            ViewBag.Teams = new SelectList(new[] { "Titans", "Code Warriors", "Bug Busters", "Pixel Pioneers", "Cloud Crusaders", "Dev Dynamos", "RH Operations" });
            ViewBag.Countries = new SelectList(new[] { "Brazil", "United States", "Italy", "Japan", "Canada" });
            ViewBag.Positions = new SelectList(new[] { "Dev Sênior", "Dev Pleno", "QA Sênior", "Project Manager", "HR Analyst", "Intern", "Designer UX/UI", "Sales Analyst", "Administrative Assistant", "QA Tester", "Marketing Manager", "HR Consultant", "Dev Back-end", "UX Strategist", "Cloud Engineer", "Full Stack Dev" });

            // Retorna o ViewModel de cadastro (o ColaboradorViewModel que criamos)
            return View("CadastrarUsuario", new ColaboradorViewModel() { Status = "Ativo" });
        }



        // --- ROTA: /Registro/NovoColaborador (POST - Salvar Dados) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovoColaborador(ColaboradorViewModel model)
        {
            // [IMPORTANTE]: O atributo [Required] no Model deve validar Senha, mas este check ajuda contra warnings/erros.
            if (model.Senha == null)
            {
                ModelState.AddModelError("Senha", "A senha é obrigatória.");
            }

            if (ModelState.IsValid)
            {
                string photoUrl = "https://randomuser.me/api/portraits/lego/1.jpg";

                // 1. LÓGICA DE UPLOAD DE FOTO (SE FOR UM ARQUIVO)
                if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                {
                    // O código de upload de arquivo está correto, mas precisa do 'using System.IO;'
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
                    // 1b. LÓGICA DE FOTO (SE FOR UMA URL)
                    // Se não houve upload de arquivo, mas uma URL foi fornecida
                    photoUrl = model.PhotoUrl;
                }

                // 2. CRIPTOGRAFIA E LÓGICA DE MANAGER
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Senha!);
                string finalPosition = model.IsManager ? "Project Manager" : model.Position!;

                // 3. MAPEAR E SALVAR
                var novoColaborador = new Colaborador
                {
                    FullName = model.FullName!,
                    Email = model.Email!,
                    PasswordHash = hashedPassword,
                    Position = finalPosition,
                    Department = model.Department,
                    TeamName = model.TeamName,
                    Country = model.Country,
                    PhotoUrl = photoUrl, // Usa a URL salva ou o default
                    Status = model.Status ?? "Ativo" // Usa o status do rádio button
                };

                _context.Colaboradores.Add(novoColaborador);
                await _context.SaveChangesAsync();

                return RedirectToAction("Listar", "Colaboradores");
            }

            // Se o Model não for válido, recarrega os dropdowns e a View com erros
            PopulateViewBags(); // Chamada do seu método
            return View("CadastrarUsuario", model);
        }




        // --- MÉTODOS PARA A NOVA TABELA (TabelaTeste) ---

        [Authorize(Roles = "HR Manager, HR Analyst, HR Consultant")]
        [HttpGet]
        public async Task<IActionResult> ListarDados()
        {
            ViewData["Title"] = "Listagem de Dados de Teste";
            ViewData["Profile"] = "Rh";

            // CORREÇÃO: Usar await e ToListAsync()
            var dados = await _context.DadosDeTeste.ToListAsync();

            return View(dados);
        }


        [Authorize(Roles = "HR Manager, HR Analyst, HR Consultant")]
        [HttpGet]
        public IActionResult CriarDado()
        {
            ViewData["Title"] = "Criar Novo Dado de Teste";
            ViewData["Profile"] = "Rh";
            return View(); // Retorna a View 'CriarDado.cshtml'
        }


        [Authorize(Roles = "HR Manager, HR Analyst, HR Consultant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarDado([Bind("NomeDoDado,Quantidade")] TabelaTeste dado)
        {
            if (ModelState.IsValid)
            {
                dado.DataCriacao = DateTime.Now; // Define a data de criação
                _context.DadosDeTeste.Add(dado);
                await _context.SaveChangesAsync();

                // Redireciona para a lista após salvar
                return RedirectToAction(nameof(ListarDados));
            }

            // Se a validação falhar
            return View(dado);
        }


    }
}