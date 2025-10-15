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
    // Novo Controller dedicado à recuperação de senha
    public class RecoveryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        // Construtor com injeção de dependência
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

            // Mensagem de segurança padrão (em inglês, para ser consistente)
            string successMessage = "If that email address is associated with an account, a password recovery link has been successfully sent. Please check your spam folder.";

            // 1. O colaborador NÃO existe (Camuflagem de Segurança)
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
                // Token não fornecido na URL
                return RedirectToAction("Index", "Login"); // Redireciona para o login se não houver token
            }

            // Passa o token para a View, escondido
            return View(new ResetPasswordModel { Token = token });
        }

        // =======================================================
        // PASSO 4: Receber a nova senha e finalizar a troca
        // =======================================================
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

            // 1. Validar Token e Expiração
            if (colaborador == null || colaborador.ResetPasswordTokenExpiry < DateTime.UtcNow)
            {
                // Token não encontrado, ou expirado
                ModelState.AddModelError("", "O link de recuperação é inválido ou expirou. Tente novamente.");
                return View(model);
            }

            // 2. Hash da Nova Senha e Limpeza do Token
            colaborador.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            colaborador.ResetPasswordToken = null;
            colaborador.ResetPasswordTokenExpiry = null; // Limpa o token para que não seja reutilizado

            await _context.SaveChangesAsync();

            // 3. Redirecionar para o Login com mensagem de sucesso
            TempData["SuccessMessage"] = "Sua senha foi redefinida com sucesso. Faça login.";
            return RedirectToAction("Index", "Login"); // Volta para a página de Login
        }
    }
}