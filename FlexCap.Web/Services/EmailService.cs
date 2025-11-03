// Nome do arquivo: IEmailService.cs
// Localização: FlexCap.Web.Services/

using System.Threading.Tasks;

namespace FlexCap.Web.Services
{
    // Define a interface para qualquer serviço de e-mail/notificação.
    public interface IEmailService
    {
        /// <summary>
        /// Envia um e-mail assíncrono para o endereço especificado (usado para funções genéricas como Recuperação de Senha).
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string message);

        /// <summary>
        /// Envia uma notificação de Workflow (usado pelo RequestService).
        /// </summary>
        /// <param name="userId">ID do usuário colaborador.</param>
        /// <param name="subject">Assunto do e-mail de notificação.</param>
        /// <param name="body">Corpo do e-mail.</param>
        Task SendNotificationAsync(int userId, string subject, string body);
    }
}