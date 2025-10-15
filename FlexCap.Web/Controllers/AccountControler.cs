//Page Mudar senha após o login
using BCrypt.Net;
using FlexCap.Web.Data;
using FlexCap.Web.Models.Account;
using Microsoft.AspNetCore.Authentication; 
using Microsoft.AspNetCore.Authentication.Cookies; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FlexCap.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
         
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Index", "Login");
            }

            var colaborador = await _context.Colaboradores
                                            .FirstOrDefaultAsync(c => c.Id == userId);

            if (colaborador == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword!, colaborador.PasswordHash))
            {
                ModelState.AddModelError("OldPassword", "The current password entered is incorrect.");
                return View(model);
            }

            if (BCrypt.Net.BCrypt.Verify(model.NewPassword!, colaborador.PasswordHash))
            {
                ModelState.AddModelError("NewPassword", "The new password must be different from the current one.");
                return View(model);
            }

            colaborador.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword!);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "Your password has been changed successfully! Please log in with your new password.";
            return RedirectToAction("Index", "Login");

        }
    }
}