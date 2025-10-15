using BCrypt.Net;
using FlexCap.Web.Data;
using FlexCap.Web.Models;
using FlexCap.Web.Models.Account;
using FlexCap.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FlexCap.Web.Controllers
{
    public class RecoveryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        public RecoveryController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordModel());
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var colaborador = await _context.Colaboradores
                                            .FirstOrDefaultAsync(c => c.Email == model.EmailAddress);
            string successMessage = "If that email address is associated with an account, a password recovery link has been successfully sent. Please check your spam folder.";

            if (colaborador == null)
            {
                TempData["SuccessMessage"] = successMessage;

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            string token = Guid.NewGuid().ToString();
            colaborador.ResetPasswordToken = token;
            colaborador.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(30);
            await _context.SaveChangesAsync();

            string resetUrl = Url.Action(
                "ResetPassword",
                "Recovery",
                new { token = token },
                protocol: Request.Scheme
            )!;

            string subject = "Password Recovery - FlexCap";
            string message = $"Hello,\n\nClick the link below to reset your password. The link expires in 30 minutes.\n\n{resetUrl}\n\nIf you did not request a password change, please ignore this email.";
            await _emailService.SendEmailAsync(model.EmailAddress, subject, message);

            TempData["SuccessMessage"] = successMessage; 

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }



        [HttpGet]
        public IActionResult ResetPassword(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login"); 
            }

            return View(new ResetPasswordModel { Token = token });
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var colaborador = await _context.Colaboradores
                                            .FirstOrDefaultAsync(c => c.ResetPasswordToken == model.Token);

            if (colaborador == null || colaborador.ResetPasswordTokenExpiry < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "The recovery link is invalid or expired. Please try again.");
                return View(model);
            }

            colaborador.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            colaborador.ResetPasswordToken = null;
            colaborador.ResetPasswordTokenExpiry = null; 

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Your password has been successfully reset. Please log in.";
            return RedirectToAction("Index", "Login"); 
        }
    }
}