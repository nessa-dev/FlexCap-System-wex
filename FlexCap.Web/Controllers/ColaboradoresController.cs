using FlexCap.Web.Data;
using FlexCap.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;

namespace FlexCap.Web.Controllers
{
    public class ColaboradoresController : Controller
    {
        private readonly AppDbContext _context;

        public ColaboradoresController(AppDbContext context)
        {
            _context = context;
        }


        // --- MÉTODO SEED ---
        public IActionResult Seed()
        {
            // 1. LIMPAR E CRIAR O BANCO PRIMEIRO (CORREÇÃO DA SEQUÊNCIA)
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Agora, o banco está novo e vazio.

            // 2. INSERIR TIPOS DE REQUISIÇÃO
            var requestTypes = new List<RequestType>
                {
                   new RequestType { Id = 1, Name = "Medical Leave" },
                   new RequestType { Id = 2, Name = "Personal Licence" },
                   new RequestType { Id = 3, Name = "Vacation" },
                   new RequestType { Id = 4, Name = "Day Off" },
                   new RequestType { Id = 5, Name = "Maternity Leave" },
                   new RequestType { Id = 6, Name = "Suspension" },
                   new RequestType { Id = 7, Name = "Other" }
                };
            _context.Set<RequestType>().AddRange(requestTypes);
            _context.SaveChanges();

            // 3. INSERIR COLABORADORES
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test1234");
            string hrPasswordHash = BCrypt.Net.BCrypt.HashPassword("HR2025!");
            string managerPasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager2025!");

            var colaboradores = new List<Colaborador>
                {

            new Colaborador {
                FullName = "Recursos Humanos Manager",
                Email = "recursoshumanos@flexcap.com",
                Position = "HR Manager",
                Department = "RH",
                Status = "Active",
                PhotoUrl = string.Empty,
                Country = string.Empty,
                TeamName = "RH Operations",
                PasswordHash = hrPasswordHash
            },


            new Colaborador {
            FullName = "Carlos Manager",
            Email = "carlos.manager@flexcap.com",
            Position = "Project Manager",
            Department = "Mobility",
            Status = "Active",
            PhotoUrl = "https://randomuser.me/api/portraits/men/2.jpg",
            Country = "United States",
            TeamName = "Code Warriors",
            PasswordHash = managerPasswordHash
        },

            new Colaborador {
            FullName = "Júlia Freitas",
            Email = "julia.freitas@flexcap.com",
            Position = "Dev Pleno",
            Department = "Benefits",
            Status = "Active",
            PhotoUrl = "https://randomuser.me/api/portraits/women/44.jpg",
            Country = "Brazil",
            TeamName = "Titans",
            PasswordHash = hashedPassword
                    },
            new Colaborador {
                FullName = "Jon Brown",
                Email = "jon.brown@flexcap.com",
                Position = "QA Sênior",
                Department = "Mobility",
                Status = "Inactive",
                PhotoUrl = "https://randomuser.me/api/portraits/men/45.jpg",
                Country = "United States",
                TeamName = "Code Warriors",
                PasswordHash = hashedPassword

            },
            new Colaborador {
                FullName = "Pedro Souza",
                Email = "pedro.souza@flexcap.com",
                Position = "Dev Sênior",
                Department = "Corporate Payments",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/men/46.jpg",
                Country = "Japan",
                TeamName = "Bug Busters",
                PasswordHash = hashedPassword

            },
            new Colaborador {
                FullName = "Mariana Gomes",
                Email = "mariana.gomes@flexcap.com",
                Position = "HR Analyst",
                Department = "Benefits",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/women/48.jpg",
                Country = "Italy",
                TeamName = "Code Warriors",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Lucas Santos",
                Email = "lucas.santos@flexcap.com",
                Position = "Project Manager",
                Department = "Mobility",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/men/49.jpg",
                Country = "Canada",
                TeamName = "Bug Busters",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Ana Lima",
                Email = "ana.lima@flexcap.com",
                Position = "Designer UX/UI",
                Department = "Corporate Payments",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/women/50.jpg",
                Country = "Brazil",
                TeamName = "Titans",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Fernando Costa",
                Email = "fernando.costa@flexcap.com",
                Position = "Dev Back-end",
                Department = "Mobility",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/men/51.jpg",
                Country = "Italy",
                TeamName = "Code Warriors",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Patrícia Nunes",
                Email = "patricia.nunes@flexcap.com",
                Position = "Intern",
                Department = "Corporate Payments",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/women/52.jpg",
                Country = "Japan",
                TeamName = "Bug Busters",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Gabriel Rocha",
                Email = "gabriel.rocha@flexcap.com",
                Position = "Sales Analyst",
                Department = "Benefits",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/men/53.jpg",
                Country = "Canada",
                TeamName = "Bug Busters",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Isabela Ribeiro",
                Email = "isabela.ribeiro@flexcap.com",
                Position = "Administrative Assistant",
                Department = "Benefits",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/women/54.jpg",
                Country = "Italy",
                TeamName = "Bug Busters",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Gustavo Martins",
                Email = "gustavo.martins@flexcap.com",
                Position = "QA Tester",
                Department = "Mobility",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/men/55.jpg",
                Country = "Canada",
                TeamName = "Code Warriors",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Daniela Souza",
                Email = "daniela.souza@flexcap.com",
                Position = "Marketing Manager",
                Department = "Benefits",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/women/56.jpg",
                Country = "Canada",
                TeamName = "Code Warriors",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Thiago Pereira",
                Email = "thiago.pereira@flexcap.com",
                Position = "Intern",
                Department = "Benefits",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/men/57.jpg",
                Country = "United States",
                TeamName = "Titans",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Carla Almeida",
                Email = "carla.almeida@flexcap.com",
                Position = "HR Consultant",
                Department = "Corporate Payments",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/women/58.jpg",
                Country = "United States",
                TeamName = "Titans",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Rodrigo Oliveira",
                Email = "rodrigo.oliveira@flexcap.com",
                Position = "Dev Back-end",
                Department = "Mobility",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/men/59.jpg",
                Country = "United States",
                TeamName = "Titans",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Maria Nova",
                Email = "maria.nova@flexcap.com",
                Position = "UX Strategist",
                Department = "Benefits",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/women/60.jpg",
                Country = "Brazil",
                TeamName = "Pixel Pioneers",
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Alex Cloud",
                Email = "alex.cloud@flexcap.com",
                Position = "Cloud Engineer",
                Department = "Mobility",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/men/60.jpg",
                Country = "Canada",
                TeamName = "Cloud Crusaders" ,
                PasswordHash = hashedPassword
            },
            new Colaborador {
                FullName = "Diana Dev",
                Email = "diana.dev@flexcap.com",
                Position = "Full Stack Dev",
                Department = "Corporate Payments",
                Status = "Active",
                PhotoUrl = "https://randomuser.me/api/portraits/women/61.jpg",
                Country = "United States",
                TeamName = "Dev Dynamos",
                PasswordHash = hashedPassword
            },
        };
            _context.Colaboradores.AddRange(colaboradores);
            _context.SaveChanges(); // Salva os colaboradores

            // ----------------------------------------------------
            // 4. RETORNO
            // ----------------------------------------------------

            // Remove o bloco 'if (Database already contains...)' que estava no final, pois o teste está no topo.
            return RedirectToAction("Index", "Login");
        }






        // Listar Colaboradores 
        [Authorize(Roles = "HR Manager, HR Analyst, HR Consultant")]
        public IActionResult Rh()
        {
            ViewData["Title"] = "Gestão de Colaboradores (RH)";
            ViewData["Profile"] = "Rh";
            var colaboradores = _context.Colaboradores
                .Where(c => c.Email != "recursoshumanos@flexcap.com")
                .ToList();
            ViewData["TotalColaboradores"] = colaboradores.Count;
            ViewData["TotalSetores"] = colaboradores.Select(c => c.Department).Distinct().Count();
            return RedirectToAction("Index", "Home");
        }






        public IActionResult Listar()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrEmpty(userEmail))
            {
                var colaboradorLogado = _context.Colaboradores
                                                .AsNoTracking()
                                                .FirstOrDefault(c => c.Email == userEmail);

                if (colaboradorLogado != null)
                {
                    if (colaboradorLogado.Position == "Project Manager")
                    {
                        ViewData["Profile"] = "Manager";
                    }
                    else if (colaboradorLogado.Position == "HR Manager" || colaboradorLogado.Position == "HR Analyst" || colaboradorLogado.Position == "HR Consultant")
                    {
                        ViewData["Profile"] = "Rh";
                    }
                    else
                    {
                        ViewData["Profile"] = "Colaborador";
                    }
                }
                else
                {
                    ViewData["Profile"] = "Colaborador";
                }
            }
            else
            {
                ViewData["Profile"] = "Colaborador";
            }

            ViewData["Title"] = "All Employees";
            var colaboradores = _context.Colaboradores
                .Where(c => c.Email != "recursoshumanos@flexcap.com")
                .ToList();

            return View("Rh", colaboradores);
        }



        // CRUD COMPLETO 
        public IActionResult CrudCompleto()
        {
            ViewData["Title"] = "Administração Completa (CRUD)";
            var colaboradores = _context.Colaboradores.ToList();
            return View("ListarUsuarios", colaboradores);
        }


        // Editar Colaborador (GET) 
        public IActionResult Editar(int id)
        {
            var colaborador = _context.Colaboradores.Find(id);
            if (colaborador == null) return NotFound();
            ViewBag.Departments = new SelectList(new[] { "Benefits", "Mobility", "Corporate Payments" });
            ViewBag.Teams = new SelectList(new[] { "Code Warriors", "Bug Busters", "Titans", "Pixel Pioneers", "Cloud Crusaders", "Dev Dynamos" });
            ViewBag.Countries = new SelectList(new[] { "Brazil", "United States", "Italy", "Japan", "Canada" });
            return View("EditarUsuario", colaborador);
        }

        // Editar Colaborador (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar([Bind("Id,FullName,Email,Position,Department,TeamName,Country")] Colaborador colaborador)
        {
            if (ModelState.IsValid)
            {
                var colaboradorOriginal = _context.Colaboradores.Find(colaborador.Id);
                if (colaboradorOriginal == null) return NotFound();

                colaboradorOriginal.FullName = colaborador.FullName;
                colaboradorOriginal.Email = colaborador.Email;
                colaboradorOriginal.Position = colaborador.Position;
                colaboradorOriginal.Department = colaborador.Department;
                colaboradorOriginal.TeamName = colaborador.TeamName;
                colaboradorOriginal.Country = colaborador.Country;

                _context.Entry(colaboradorOriginal).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Listar");
            }
            ViewBag.Departments = new SelectList(new[] { "Benefits", "Mobility", "Corporate Payments" });
            ViewBag.Teams = new SelectList(new[] { "Code Warriors", "Bug Busters", "Titans", "Pixel Pioneers", "Cloud Crusaders", "Dev Dynamos" });
            ViewBag.Countries = new SelectList(new[] { "Brazil", "United States", "Italy", "Japan", "Canada" });

            return View("EditarUsuario", colaborador);
        }

        // Excluir Colaborador (GET - confirmação)
        public IActionResult Excluir(int id)
        {
            var colaborador = _context.Colaboradores.Find(id);
            if (colaborador == null) return NotFound();
            return View("ExcluirUsuario", colaborador);
        }

        // Excluir Colaborador (POST - confirmação final)
        [HttpPost]
        public IActionResult Excluir(int id, [Bind("Id")] Colaborador colaborador)
        {
            var colaboradorToDelete = _context.Colaboradores.Find(id);
            if (colaboradorToDelete == null) return NotFound();

            _context.Colaboradores.Remove(colaboradorToDelete);
            _context.SaveChanges();
            return RedirectToAction("Listar");
        }




        [HttpGet]
        public async Task<IActionResult> Colaborador()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Logout", "Login");
            }

            var colaboradorLogado = await _context.Colaboradores
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (colaboradorLogado == null)
            {
                return NotFound("Employee profile not found.");
            }

            var equipeDoUsuario = colaboradorLogado.TeamName ?? "No Team";

            var membrosAtivos = await _context.Colaboradores
                                                .CountAsync(c => c.TeamName == equipeDoUsuario && c.Status == "Active");
            var totalMembrosTime = await _context.Colaboradores
                                                .CountAsync(c => c.TeamName == equipeDoUsuario);

            ViewData["FirstName"] = colaboradorLogado.FullName?.Split(' ')[0] ?? "Colaborador";
            ViewData["UserTeam"] = equipeDoUsuario;
            ViewData["ActiveMembers"] = membrosAtivos;
            ViewData["TotalMembers"] = totalMembrosTime;
            ViewData["Profile"] = "Colaborador";
            return View("~/Views/Home/colaborador.cshtml");
        }





        [HttpGet]
        public async Task<IActionResult> ListarEquipe()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Index", "Login");
            }

            var colaboradorLogado = await _context.Colaboradores
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (colaboradorLogado == null)
            {
                return NotFound("The logged-in employee's profile could not be found.");
            }

            var equipeDoUsuario = colaboradorLogado.TeamName;
            string perfil = colaboradorLogado.Position == "Project Manager" ? "Manager" : "Colaborador";

            var colaboradoresDoTime = await _context.Colaboradores
                .AsNoTracking()
                .Where(c => c.TeamName == equipeDoUsuario)

                .OrderByDescending(c => c.FullName == colaboradorLogado.FullName)
                .ThenByDescending(c => c.Position == "Project Manager")
                .ThenBy(c => c.FullName)
                .ToListAsync();

            ViewData["Title"] = $"Membros da Equipe: {equipeDoUsuario}";
            ViewData["Profile"] = perfil;

            ViewData["LoggedInUserName"] = colaboradorLogado.FullName?.Trim();

            return View("TimeDetalhes", colaboradoresDoTime);
        }





        [Authorize(Roles = "HR Manager, HR Analyst, HR Consultant")]
        public IActionResult BuscarColaboradores(string nomeBusca, string statusBusca, string countryBusca, string sectorBusca, string teamBusca)
        {
            ViewData["Title"] = "Gestão de Colaboradores (RH)";
            ViewData["Profile"] = "Rh";

            var colaboradores = _context.Colaboradores
                .Where(c => c.Email != "recursoshumanos@flexcap.com")
                .AsQueryable();

            if (!string.IsNullOrEmpty(nomeBusca))
            {
                colaboradores = colaboradores.Where(c => c.FullName != null && c.FullName.Contains(nomeBusca));
            }
            if (!string.IsNullOrEmpty(statusBusca) && statusBusca != "All")
            {
                colaboradores = colaboradores.Where(c => c.Status == statusBusca);
            }
            if (!string.IsNullOrEmpty(countryBusca) && countryBusca != "Select country")
            {
                colaboradores = colaboradores.Where(c => c.Country == countryBusca);
            }
            if (!string.IsNullOrEmpty(sectorBusca) && sectorBusca != "Select Sector")
            {
                colaboradores = colaboradores.Where(c => c.Department == sectorBusca);
            }
            if (!string.IsNullOrEmpty(teamBusca) && teamBusca != "Select Team")
            {
                colaboradores = colaboradores.Where(c => c.TeamName == teamBusca);
            }

            ViewData["NomeBusca"] = nomeBusca;
            ViewData["StatusBusca"] = statusBusca;
            ViewData["CountryBusca"] = countryBusca;
            ViewData["SectorBusca"] = sectorBusca;
            ViewData["TeamBusca"] = teamBusca;

            return View("Rh", colaboradores.ToList());
        }


        [HttpGet]
        [Authorize(Roles = "Project Manager")]
        public async Task<IActionResult> Manager()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Index", "Login");
            }
            var currentUser = await _context.Colaboradores
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (currentUser == null)
            {
                return NotFound("User profile not found.");
            }

            var equipeDoUsuario = currentUser.TeamName;

            var totalMembrosTime = await _context.Colaboradores
                                            .CountAsync(c => c.TeamName == equipeDoUsuario);

            var membrosAtivos = await _context.Colaboradores
                                            .CountAsync(c => c.TeamName == equipeDoUsuario && c.Status == "Active");

            ViewData["FirstName"] = currentUser.FullName.Split(' ')[0];
            ViewData["UserTeam"] = equipeDoUsuario;
            ViewData["ActiveMembers"] = membrosAtivos;
            ViewData["TotalMembers"] = totalMembrosTime;

            ViewData["Title"] = "Home Manager";

            return View("~/Views/Home/Manager.cshtml");
        }






    }
}