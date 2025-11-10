using FlexCap.Web.Data;
using FlexCap.Web.Models;
using FlexCap.Web.Models.Requests;
using FlexCap.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace FlexCap.Web.Services
{
    public class RequestService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public RequestService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        private async Task NotifyCollaboratorAsync(RequestEntity request, string decisionMaker)
        {
            string subject = "";
            string body = "";

            switch (request.Status)
            {
                case "Waiting For Manager":
                    subject = "Request Submitted - Awaiting Manager Review";
                    body = $"Your request '{request.Subject}' has been submitted and is awaiting approval by your Manager.";
                    break;
                case "Waiting For HR":
                    subject = "Manager Approved - Awaiting Final HR Validation";
                    body = $"Your request '{request.Subject}' was approved by your Manager and is now with HR for final validation.";
                    break;
                case "Adjustment Requested": // <<< NOVO STATUS PARA O FLUXO DE AJUSTE
                    subject = "Request Requires Adjustment/Correction";
                    body = $"Your request '{request.Subject}' requires adjustment by you, as requested by {decisionMaker}. Details: {request.RejectionReason}";
                    break;
                case "Approved":
                    subject = "SUCCESS! Request Fully Approved";
                    body = $"Congratulations! Your request '{request.Subject}' was fully approved and is now being processed.";
                    break;
                case "Rejected":
                    subject = "Request Rejected";
                    // Usamos RejectionReason para justificativas de rejeição e de ajuste
                    body = $"Your request '{request.Subject}' was rejected by {decisionMaker}. Reason: {request.RejectionReason}";
                    break;

                default:
                    return;
            }
            await _emailService.SendNotificationAsync(request.CollaboratorId, subject, body);
        }

        public async Task<int> GetV()
        {
            return await _context.SaveChangesAsync();
        }



        public async Task<int> SubmitNewRequest(AbsenceRequestSubmitViewModel model, int collaboratorId)
        {
            var colaborador = await _context.Colaboradores.FirstOrDefaultAsync(c => c.Id == collaboratorId);
            if (colaborador == null)
                throw new InvalidOperationException("Collaborator not found.");

            var managerDaEquipe = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.TeamName == colaborador.TeamName && c.Position == "Project Manager");

            if (managerDaEquipe == null)
                throw new InvalidOperationException($"Project Manager not found for team {colaborador.TeamName}.");

            // 🟢 Upload do arquivo PDF (opcional)
            string? savedAttachmentPath = null;
            if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.AttachmentFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AttachmentFile.CopyToAsync(stream);
                }

                savedAttachmentPath = $"/Uploads/{uniqueFileName}";
            }

            // 🟢 Criação do request
            var newRequest = new RequestEntity
            {
                CollaboratorId = collaboratorId,
                Subject = model.Subject,
                TypeId = model.TypeId,
                StartDate = model.StartDate.Value,
                EndDate = model.EndDate.Value,
                Description = model.Description,
                Status = "Waiting For Manager",
                CurrentManagerId = managerDaEquipe.Id,
                CreationDate = DateTime.Now,
                LastUpdateDate = DateTime.Now,
                AttachmentPath = savedAttachmentPath // pode ser null se não tiver arquivo
            };

            _context.Requests.Add(newRequest);
            await _context.SaveChangesAsync();

            await NotifyCollaboratorAsync(newRequest, "System");

            return newRequest.Id;
        }





        public async Task ProcessManagerDecision(int requestId, int managerUserId, string actionType, string justification)
        {
            // CORREÇÃO ESSENCIAL: Garante que o valor não é NULL para ser usado no Log
            string logComment = justification ?? "";

            var request = await _context.Requests.FindAsync(requestId);

            // 1. Validação de Status (OK)
            if (request == null || request.Status != "Waiting For Manager")
            {
                throw new InvalidOperationException("Request not pending Manager approval.");
            }

            string newStatus = string.Empty;
            string actionDescription = string.Empty;

            // Lógica de Transição de Status
            switch (actionType.ToLower())
            {
                case "approve": newStatus = "Waiting For HR"; actionDescription = "Manager Approved"; break;
                case "reject": newStatus = "Rejected"; actionDescription = "Manager Rejected"; request.RejectionReason = logComment; break; // Use logComment aqui também
                case "requestadjustment": newStatus = "Adjustment Requested"; actionDescription = "Manager Requested Adjustment"; request.RejectionReason = logComment; break; // Use logComment aqui também
                default: throw new ArgumentException("Invalid action type.");
            }

            request.Status = newStatus;
            request.CurrentManagerId = managerUserId;
            request.LastUpdateDate = DateTime.Now;

            // 2. Descomentar e Ativar o Log (ESSENCIAL)
            _context.RequestLogs.Add(new RequestLogEntity
            {
                RequestId = requestId,
                ActionByUserId = managerUserId,
                ActionType = actionDescription,
                NewStatus = newStatus,
                ActionTimestamp = DateTime.Now,
                // CORREÇÃO CRÍTICA: Use a variável segura contra NULL
                Comment = logComment // Agora, se a aprovação for enviada, o valor é "", não NULL
            });

            // 3. Salvar as Mudanças (Muda o Status no DB)
            await _context.SaveChangesAsync();

            // 4. Notificação (Comente se der problema de travamento SMTP)
            await NotifyCollaboratorAsync(request, "Manager");
        }





        public async Task<int> GetPendingHRRequestsCountAsync()
        {
            return await _context.Requests
                .CountAsync(r => r.Status == "Waiting For HR");
        }



        public async Task ProcessHRDecision(int requestId, int hrUserId, string actionType, string justification)
        {
            // 🛑 CORREÇÃO CRÍTICA: Garante que o justification é uma string vazia ("") se for nulo.
            string logComment = justification ?? "";

            var request = await _context.Requests.FindAsync(requestId);

            // 1. Validação de Status (OK)
            if (request == null || request.Status != "Waiting For HR")
            {
                throw new InvalidOperationException("Request not pending HR validation.");
            }

            string newStatus = string.Empty;
            string actionDescription = string.Empty;

            // Lógica de Transição de Status
            switch (actionType.ToLower())
            {
                case "approve":
                    newStatus = "Approved";
                    actionDescription = "HR Approved";
                    // O logComment será "" aqui.
                    break;

                case "reject":
                    newStatus = "Rejected";
                    actionDescription = "HR Rejected";
                    request.RejectionReason = logComment; // Usa a justificativa para a razão
                    break;

                case "returntomanager":
                    newStatus = "Waiting For Manager";
                    actionDescription = "HR Returned to Manager";
                    request.RejectionReason = logComment; // Usa a justificativa para a razão
                    break;

                default:
                    throw new ArgumentException("Invalid action type.");
            }

            request.Status = newStatus;
            request.CurrentHRId = hrUserId;
            request.LastUpdateDate = DateTime.Now;

            // 2. Registro do Log (Agora usa a variável segura logComment)
            _context.RequestLogs.Add(new RequestLogEntity
            {
                RequestId = requestId,
                ActionByUserId = hrUserId,
                ActionType = actionDescription,
                NewStatus = newStatus,
                ActionTimestamp = DateTime.Now,
                // CORREÇÃO: Usa a variável segura logComment
                Comment = logComment
            });

            // 3. Salvar as Mudanças (Persiste o status final e o log)
            await _context.SaveChangesAsync();

            // 4. Notificação
            await NotifyCollaboratorAsync(request, "HR");
        }
    }
}