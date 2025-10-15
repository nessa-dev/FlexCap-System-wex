using BCrypt.Net;
using FlexCap.Web.Data;
using FlexCap.Web.Models;
using FlexCap.Web.Models.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting; 
using System.IO;                  
using System.Threading.Tasks;     
using System;                     
namespace FlexCap.Web.Controllers
{
    public class CadastroController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; 

        public CadastroController(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public IActionResult Cadastro()
        {
            return View(new CadastroModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastro(CadastroModel model)
        {
            if (model.ProfilePhotoFile == null || model.ProfilePhotoFile.Length == 0)
            {
                ModelState.AddModelError("ProfilePhotoFile", "The Profile Photo field is required.");
            }

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
                    string photoUrl = string.Empty;


                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string uploadPath = Path.Combine(wwwRootPath, "images", "profiles");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string extension = Path.GetExtension(model.ProfilePhotoFile!.FileName);
                    string uniqueFileName = Guid.NewGuid().ToString() + extension;

                    string filePath = Path.Combine(uploadPath, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePhotoFile.CopyToAsync(fileStream);
                    }

                    
                    photoUrl = Path.Combine("/images", "profiles", uniqueFileName).Replace("\\", "/");


                    var novoColaborador = new Colaborador
                    {
                        FullName = model.FullName!,
                        Email = model.EmailAddress!,
                        Position = model.Position ?? "",
                        Department = model.Department ?? "",
                        TeamName = model.Team ?? "",
                        Country = model.CountryOfOperation ?? "",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password!),

                        PhotoUrl = photoUrl
                    };

                    _context.Colaboradores.Add(novoColaborador);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Login");
                }
            }

            return View(model);
        }

    }
}