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
                case "Adjustment Requested": 
                    subject = "Request Requires Adjustment/Correction";
                    body = $"Your request '{request.Subject}' requires adjustment by you, as requested by {decisionMaker}. Details: {request.RejectionReason}";
                    break;
                case "Approved":
                    subject = "SUCCESS! Request Fully Approved";
                    body = $"Congratulations! Your request '{request.Subject}' was fully approved and is now being processed.";
                    break;
                case "Rejected":
                    subject = "Request Rejected";
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

            // Upload do arquivo PDF (opcional)
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

            // Criação do request
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
                AttachmentPath = savedAttachmentPath 
            };

            _context.Requests.Add(newRequest);
            await _context.SaveChangesAsync();

            await NotifyCollaboratorAsync(newRequest, "System");

            return newRequest.Id;
        }

        public async Task ProcessManagerDecision(int requestId, int managerUserId, string actionType, string justification)
        {
            string logComment = justification ?? "";

            var request = await _context.Requests.FindAsync(requestId);

            // Validação de Status 
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

            _context.RequestLogs.Add(new RequestLogEntity
            {
                RequestId = requestId,
                ActionByUserId = managerUserId,
                ActionType = actionDescription,
                NewStatus = newStatus,
                ActionTimestamp = DateTime.Now,
                Comment = logComment 
            });

            // 3. Salvar as Mudanças (Muda o Status no DB)
            await _context.SaveChangesAsync();

            // 4. Notificação 
            await NotifyCollaboratorAsync(request, "Manager");
        }


        public async Task<int> GetPendingHRRequestsCountAsync()
        {
            return await _context.Requests
                .CountAsync(r => r.Status == "Waiting For HR");
        }



        public async Task ProcessHRDecision(int requestId, int hrUserId, string actionType, string justification)
        {
            string logComment = justification ?? "";

            var request = await _context.Requests.FindAsync(requestId);

            // Validação de Status 
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
                    break;

                case "reject":
                    newStatus = "Rejected";
                    actionDescription = "HR Rejected";
                    request.RejectionReason = logComment; 
                    break;

                case "returntomanager":
                    newStatus = "Waiting For Manager";
                    actionDescription = "HR Returned to Manager";
                    request.RejectionReason = logComment; 
                    break;

                default:
                    throw new ArgumentException("Invalid action type.");
            }

            request.Status = newStatus;
            request.CurrentHRId = hrUserId;
            request.LastUpdateDate = DateTime.Now;

            _context.RequestLogs.Add(new RequestLogEntity
            {
                RequestId = requestId,
                ActionByUserId = hrUserId,
                ActionType = actionDescription,
                NewStatus = newStatus,
                ActionTimestamp = DateTime.Now,
                Comment = logComment
            });

            // 3. Salvar as Mudanças 
            await _context.SaveChangesAsync();

            // 4. Notificação
            await NotifyCollaboratorAsync(request, "HR");
        }
    }
}