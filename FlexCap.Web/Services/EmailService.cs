// EM: FlexCap.Web/Services/EmailService.cs

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FlexCap.Web.Services
{
    // Interface que o RecoveryController usa
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }

    // Implementação mock (simulada) para testes
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string message)
        {
            // --- ESTE É O LOCAL ONDE O E-MAIL REAL SERIA ENVIADO (Ex: usando MailKit) ---

            // Loga a mensagem no console/terminal para fins de teste
            _logger.LogInformation($"\n--- SIMULAÇÃO DE E-MAIL ---");
            _logger.LogInformation($"TO: {toEmail}");
            _logger.LogInformation($"SUBJECT: {subject}");
            _logger.LogInformation($"BODY:\n{message}");
            _logger.LogInformation($"---------------------------\n");

            // Retorna uma tarefa concluída
            return Task.CompletedTask;
        }
    }
}