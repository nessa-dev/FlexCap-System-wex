// Nome do arquivo: MockEmailService.cs

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System; 

namespace FlexCap.Web.Services
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string message)
        {
            _logger.LogInformation($"\n--- SIMULAÇÃO DE E-MAIL GERAL ---");
            _logger.LogInformation($"TO: {toEmail}");
            _logger.LogInformation($"SUBJECT: {subject}");
            _logger.LogInformation($"---------------------------\n");
            return Task.CompletedTask;
        }
        public Task SendNotificationAsync(int userId, string subject, string body)
        {
            string simulatedEmail = $"collaborator{userId}@company.com";

            _logger.LogInformation($"\n--- NOTIFICAÇÃO DE WORKFLOW (PONTO 5) ---");
            _logger.LogInformation($"DESTINATÁRIO ID: {userId} (Email: {simulatedEmail})");
            _logger.LogInformation($"ASSUNTO: {subject}");
            _logger.LogInformation($"BODY: {body.Substring(0, Math.Min(body.Length, 80))}...");
            _logger.LogInformation($"-------------------------------------------\n");

            return Task.CompletedTask;
        }
    }
}