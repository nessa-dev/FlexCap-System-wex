using FlexCap.Web.Data;
using FlexCap.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FlexCap.Web.Controllers
{
    public class ColaboradoresController : Controller
    {
        private readonly AppDbContext _context;

        public ColaboradoresController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Seed()
        {
            if (!_context.Colaboradores.Any())
            {
                var colaboradores = new List<Colaborador>
            {
                new Colaborador { NomeCompleto = "Júlia Freitas", Email = "julia.freitas@flexcap.com", Cargo = "Dev Pleno", Setor = "Benefits", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/women/44.jpg", Pais = "Brazil", Time = "Titans" },
                new Colaborador { NomeCompleto = "Jon Brown", Email = "jon.brown@flexcap.com", Cargo = "QA Sênior", Setor = "Mobility", Status = "Inativo", FotoUrl = "https://randomuser.me/api/portraits/men/45.jpg", Pais = "United States", Time = "Code Warriors" },
                new Colaborador { NomeCompleto = "Pedro Souza", Email = "pedro.souza@flexcap.com", Cargo = "Dev Sênior", Setor = "Comportate Payments", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/men/46.jpg", Pais = "Japan", Time = "Bug Busters" },
                new Colaborador { NomeCompleto = "Mariana Gomes", Email = "mariana.gomes@flexcap.com", Cargo = "HR Analyst", Setor = "Benefits", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/women/48.jpg", Pais = "Italy", Time = "Code Warriors" },
                new Colaborador { NomeCompleto = "Lucas Santos", Email = "lucas.santos@flexcap.com", Cargo = "Project Manager", Setor = "Mobility", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/men/49.jpg", Pais = "Canada", Time = "Bug Busters" },
                new Colaborador { NomeCompleto = "Ana Lima", Email = "ana.lima@flexcap.com", Cargo = "Designer UX/UI", Setor = "Comportate Payments", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/women/50.jpg", Pais = "Brazil", Time = "Titans" },
                new Colaborador { NomeCompleto = "Fernando Costa", Email = "fernando.costa@flexcap.com", Cargo = "Dev Back-end", Setor = "Mobility", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/men/51.jpg", Pais = "Italy", Time = "Code Warriors" },
                new Colaborador { NomeCompleto = "Patrícia Nunes", Email = "patricia.nunes@flexcap.com", Cargo = "Intern", Setor = "Comportate Payments", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/women/52.jpg", Pais = "Japan", Time = "Bug Busters" },
                new Colaborador { NomeCompleto = "Gabriel Rocha", Email = "gabriel.rocha@flexcap.com", Cargo = "Sales Analyst", Setor = "Benefits", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/men/53.jpg", Pais = "Canada", Time = "Bug Busters" },
                new Colaborador { NomeCompleto = "Isabela Ribeiro", Email = "isabela.ribeiro@flexcap.com", Cargo = "Administrative Assistant", Setor = "Benefits", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/women/54.jpg", Pais = "Italy", Time = "Bug Busters" },
                new Colaborador { NomeCompleto = "Gustavo Martins", Email = "gustavo.martins@flexcap.com", Cargo = "QA Tester", Setor = "Mobility", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/men/55.jpg", Pais = "Canada", Time = "Code Warriors" },
                new Colaborador { NomeCompleto = "Daniela Souza", Email = "daniela.souza@flexcap.com", Cargo = "Marketing Manager", Setor = "Benefits", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/women/56.jpg", Pais = "Canada", Time = "Code Warriors" },
                new Colaborador { NomeCompleto = "Thiago Pereira", Email = "thiago.pereira@flexcap.com", Cargo = "Intern", Setor = "Benefits", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/men/57.jpg", Pais = "United States", Time = "Titans" },
                new Colaborador { NomeCompleto = "Carla Almeida", Email = "carla.almeida@flexcap.com", Cargo = "HR Consultant", Setor = "Comportate Payments", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/women/58.jpg", Pais = "United States", Time = "Titans" },
                new Colaborador { NomeCompleto = "Rodrigo Oliveira", Email = "rodrigo.oliveira@flexcap.com", Cargo = "Dev Back-end", Setor = "Mobility", Status = "Ativo", FotoUrl = "https://randomuser.me/api/portraits/men/59.jpg", Pais = "United States", Time = "Titans" }
            };

                foreach (var colaborador in colaboradores)
                {
                    _context.Colaboradores.Add(colaborador);
                }
                _context.SaveChanges();
            }
            return RedirectToAction("Rh");
        }

        // Listar
        public IActionResult ListarUsuarios()
        {
            ViewData["Title"] = "Todos os Usuários Cadastrados";
            var usuarios = _context.Usuarios.ToList();
            return View(usuarios);
        }






        // Editar (GET)
        public IActionResult Editar(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null) return NotFound();

            // Populando selects
            ViewBag.Departments = new SelectList(new[] { "TI", "RH", "Financeiro", "Marketing" });
            ViewBag.Teams = new SelectList(new[] { "Time A", "Time B", "Time C" });
            ViewBag.Countries = new SelectList(new[] { "Brasil", "Portugal", "EUA" });

            return View("EditarUsuario", usuario);
        }

        // Editar (POST)
        [HttpPost]
        public IActionResult Editar(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                var usuarioOriginal = _context.Usuarios.Find(usuario.Id);
                if (usuarioOriginal == null) return NotFound();

                // Atualiza apenas os campos permitidos
                usuarioOriginal.FullName = usuario.FullName;
                usuarioOriginal.EmailAddress = usuario.EmailAddress;
                usuarioOriginal.Position = usuario.Position;
                usuarioOriginal.Department = usuario.Department;
                usuarioOriginal.Team = usuario.Team;
                usuarioOriginal.CountryOfOperation = usuario.CountryOfOperation;

                // Força o EF a reconhecer a alteração
                _context.Entry(usuarioOriginal).State = EntityState.Modified;
                _context.SaveChanges();

                return RedirectToAction("ListarUsuarios");
            }

            // repopula caso dê erro
            ViewBag.Departments = new SelectList(new[] { "TI", "RH", "Financeiro", "Marketing" });
            ViewBag.Teams = new SelectList(new[] { "Time A", "Time B", "Time C" });
            ViewBag.Countries = new SelectList(new[] { "Brasil", "Portugal", "EUA" });

            return View("EditarUsuario", usuario);
        }













        // Excluir (GET - confirmação)"
        public IActionResult Excluir(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null) return NotFound();

            return View("ExcluirUsuario", usuario);
        }

        // Excluir (POST - confirmar exclusão)
        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExcluir(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();
            return RedirectToAction("ListarUsuarios");
        }




        public IActionResult Colaborador()
        {
            ViewData["Title"] = "Colaboradores da Equipe";
            ViewData["Profile"] = "Colaborador";

            string emailDoUsuarioLogado = "pedro.souza@flexcap.com";

            var colaboradorLogado = _context.Colaboradores
                                            .FirstOrDefault(c => c.Email == emailDoUsuarioLogado);

            if (colaboradorLogado != null)
            {
                var timeDoUsuario = colaboradorLogado.Time;
                var colaboradoresDoTime = _context.Colaboradores
                                                  .Where(c => c.Time == timeDoUsuario)
                                                  .ToList();
                return View(colaboradoresDoTime);
            }
            return View(new List<Colaborador>());




        }

        public IActionResult Manager()
        {
            ViewData["Title"] = "Gestão de Colaboradores";
            ViewData["Profile"] = "Manager";
            string emailDoGestorLogado = "jon.brown@flexcap.com"; 

            var gestorLogado = _context.Colaboradores
                                        .FirstOrDefault(c => c.Email == emailDoGestorLogado);

            if (gestorLogado != null)
            {
                var timeDoGestor = gestorLogado.Time;
                var colaboradoresDoTime = _context.Colaboradores
                                                    .Where(c => c.Time == timeDoGestor)
                                                    .ToList();

                return View(colaboradoresDoTime);
            }
            return View(new List<Colaborador>());
        }

        public IActionResult Rh()
        {
            ViewData["Title"] = "Gestão de Colaboradores (RH)";
            ViewData["Profile"] = "Rh";
            var colaboradores = _context.Colaboradores.ToList();
            return View(colaboradores);
        }
    }
}