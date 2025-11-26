using FlexCap.Web.Data;
using FlexCap.Web.Models;
using FlexCap.Web.Models.Sprint;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System; 
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace FlexCap.Web.Controllers
{
    public class SprintController : Controller
    {
        private readonly AppDbContext _context;

        // Construtor para Injeção de Dependência
        public SprintController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Manager()
        {
            ViewData["Title"] = "Sprints & Gantt";
            ViewData["Profile"] = "Manager";
            return View("~/Views/Sprint/Manager.cshtml"); 
        }


        [HttpGet]
        public IActionResult GetTeamMembers()
        {
            var loggedInUserIdentifier = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(loggedInUserIdentifier))
            {
                return Unauthorized("User must be logged in.");
            }
            var loggedUser = _context.Colaboradores
                .FirstOrDefault(c => c.FullName == loggedInUserIdentifier);

            if (loggedUser == null)
            {
                return Json(new List<object>());
            }

            // Identifica o nome da equipe do usuário
            var teamName = loggedUser.TeamName;

            // Busca todos os colaboradores da mesma equipe
            var sameTeamMembers = _context.Colaboradores
                .Where(c => c.TeamName == teamName)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.FullName,
                    position = c.Position,
                    photoUrl = c.PhotoUrl,
                    status = c.Status
                })
                .ToList();

            // Retorna os dados em formato JSON
            return Json(sameTeamMembers);
        }


        [HttpGet]
        public async Task<IActionResult> GetActive(int? id = null) 
        {
            // Obtém o usuário logado
            var loggedInUserName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(loggedInUserName)) return Unauthorized();

            var loggedUser = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.FullName == loggedInUserName);

            if (loggedUser == null) return NotFound();

            if (id.HasValue && id.Value > 0)
            {
                var specificSprint = await _context.Sprints
                    .FirstOrDefaultAsync(s => s.Id == id.Value && s.IsActive == true);

                if (specificSprint != null) return Json(specificSprint);
            }

            // Busca pela Sprint ATIVA da equipe
            var teamMemberIds = await _context.Colaboradores
                .Where(c => c.TeamName == loggedUser.TeamName)
                .Select(c => c.Id)
                .ToListAsync();

            var teamMemberIdStrings = teamMemberIds.Select(i => i.ToString()).ToList();

            // Busca todas as sprints ativas para fazer a checagem local
            var activeSprints = await _context.Sprints
                .Where(s => s.IsActive == true)
                .ToListAsync();

            // Itera localmente para encontrar a sprint onde o participante faz parte da equipe logada
            var teamActiveSprint = activeSprints
                .FirstOrDefault(s => teamMemberIdStrings.Any(idStr => s.ParticipatingMemberIds.Contains(idStr)));

            if (teamActiveSprint == null)
            {
                return NotFound(); // Não há sprint ativa para esta equipe
            }

            return Json(teamActiveSprint);
        }

        [HttpPut]
        [Route("Sprint/Update")]
        public async Task<IActionResult> Update([FromBody] SprintModel updatedSprint)
        {
            if (updatedSprint.Id == 0 || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //  Busca a sprint original no banco de dados
            var originalSprint = await _context.Sprints.FindAsync(updatedSprint.Id);
            if (originalSprint == null) return NotFound();

            // Atualiza apenas os campos 
            originalSprint.Name = updatedSprint.Name;
            originalSprint.Goal = updatedSprint.Goal;
            originalSprint.StartDate = updatedSprint.StartDate;
            originalSprint.EndDate = updatedSprint.EndDate;
            originalSprint.Notes = updatedSprint.Notes;

            // Salva
            _context.Sprints.Update(originalSprint);
            await _context.SaveChangesAsync();

            return NoContent(); 
        }

        [HttpDelete("Sprint/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sprintToDelete = await _context.Sprints.FindAsync(id);

            if (sprintToDelete == null) return NotFound();

            _context.Sprints.Remove(sprintToDelete);
            await _context.SaveChangesAsync();

            return NoContent(); 
        }

        [HttpPost]
        [Route("Sprint/CreateSprint")]
        public async Task<IActionResult> CreateSprint([FromBody] SprintModel model, [FromQuery] List<int> memberIds)
        {
            // Validação do Estado do Modelo
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validação simples de datas
            if (model.StartDate > model.EndDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
                return BadRequest(ModelState);
            }


            // Converte os IDs de membro para strings para a checagem no banco de dados
            var memberIdStrings = memberIds.Select(id => id.ToString()).ToList();

            // 2. Checa se algum dos membros selecionados JÁ está em outra sprint ATIVA
            var conflictingActiveSprint = await _context.Sprints
                .Where(s => s.IsActive == true)
                .ToListAsync(); 

            var conflict = conflictingActiveSprint.FirstOrDefault(s =>
                memberIdStrings.Any(idStr => s.ParticipatingMemberIds.Contains(idStr)));

            if (conflict != null)
            {
                return StatusCode(409, $"A new sprint cannot be created. At least one selected member is already participating in active sprint '{conflict.Name}' (ID: {conflict.Id}).");
            }

            model.ParticipatingMemberIds = string.Join(",", memberIds.Distinct());
            model.IsActive = true;

            _context.Sprints.Add(model);
            await _context.SaveChangesAsync();

            return Json(model);
        }


        [HttpPost]
        [Route("Sprint/Finish")]
        public async Task<IActionResult> Finish([FromBody] SprintResultModel result)
        {
            // Validação de Estado
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Salva o resultado da retrospectiva no banco (Histórico)
            _context.SprintResults.Add(result);

            // Marca a sprint original como inativa/completa
            var sprint = await _context.Sprints.FindAsync(result.SprintId);

            if (sprint != null)
            {
                sprint.IsActive = false; 
                _context.Sprints.Update(sprint);
            }

            // Salva as mudanças (SprintResult e Sprint status)
            await _context.SaveChangesAsync();
            return NoContent();
        }



        [HttpGet]
        [Route("Sprint/GetFinished")]
        public async Task<IActionResult> GetFinished()
        {
            var loggedInUserName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(loggedInUserName)) return Unauthorized();

            var loggedUser = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.FullName == loggedInUserName);

            if (loggedUser == null) return NotFound("User not found.");

            // Identifica todos os membros da equipe do Manager logado
            var teamMemberIds = await _context.Colaboradores
                .Where(c => c.TeamName == loggedUser.TeamName) // Filtra pelo nome da equipe
                .Select(c => c.Id)
                .ToListAsync();

            var teamMemberIdStrings = teamMemberIds.Select(i => i.ToString()).ToList();

            // Busca todas as Sprints que JÁ FORAM FINALIZADAS 
            var finishedSprints = await _context.Sprints
                .Where(s => s.IsActive == false)
                .ToListAsync();

            var relevantSprintIds = new HashSet<int>();

            // Filtra as Sprints que pertencem à equipe do Manager logado
            foreach (var sprint in finishedSprints)
            {
                if (teamMemberIdStrings.Any(idStr => sprint.ParticipatingMemberIds != null && sprint.ParticipatingMemberIds.Contains(idStr)))
                {
                    relevantSprintIds.Add(sprint.Id);
                }
            }

            if (!relevantSprintIds.Any())
            {
                return Json(new List<SprintResultModel>()); // Retorna vazio se não houver histórico para esta equipe
            }

            var results = await _context.SprintResults
                .Where(r => relevantSprintIds.Contains(r.SprintId))
                .OrderByDescending(r => r.ActualFinishDate)
                .Take(50) 
                .ToListAsync();

            return Json(results);
        }


        //Notificações

        private string GetIconClassByType(string type)
        {
            switch (type)
            {
                case "Holiday (Brazil)":    
                case "Holiday (EUA)":
                case "Holiday (USA)":
                    return "bi-calendar-check-fill";
                case "Corporate Training":
                    return "bi-lightbulb-fill";
                case "Online Event":
                    return "bi-laptop-fill";
                default:
                    return "bi-calendar-event";
            }
        }



        [HttpGet]
        [Route("Sprint/GetHolidayNotifications")]
        public async Task<IActionResult> GetHolidayNotifications(int? sprintId = null)
        {
            // 1) Buscar sprint ativa ou por id
            SprintModel sprint = null;

            if (sprintId.HasValue)
            {
                sprint = await _context.Sprints.FindAsync(sprintId.Value);
            }
            else
            {
                sprint = await _context.Sprints
                    .OrderByDescending(s => s.EndDate)
                    .FirstOrDefaultAsync(s => s.IsActive == true);
            }

            if (sprint == null)
            {
                return Json(new List<SprintHolidayNotificationDto>());
            }

            // Identificar membros participantes
            List<int> memberIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(sprint.ParticipatingMemberIds))
            {
                memberIds = sprint.ParticipatingMemberIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => { int.TryParse(s.Trim(), out var vid); return vid; })
                    .Where(i => i > 0)
                    .Distinct()
                    .ToList();
            }

            List<Colaborador> members;
            if (memberIds.Count > 0)
            {
                members = await _context.Colaboradores
                    .Where(c => memberIds.Contains(c.Id))
                    .ToListAsync();
            }
            else
            {
                members = await _context.Colaboradores.ToListAsync();
            }

            int totalMembers = members.Count;

            if (totalMembers == 0)
            {
                return Json(new List<SprintHolidayNotificationDto>());
            }

            // Pegar eventos do calendário que caem dentro do período da sprint
            var start = sprint.StartDate.Date;
            var end = sprint.EndDate.Date;

            var events = await _context.CalendarEvents 
                .Where(e => e.Date.Date >= start && e.Date.Date <= end)
                .OrderBy(e => e.Date)
                .ToListAsync();

            var holidayEvents = events
                .Where(e => !string.IsNullOrWhiteSpace(e.Type) && e.Type.StartsWith("Holiday", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var notifications = new List<SprintHolidayNotificationDto>();

            string ExtractCountryKey(string type)
            {
                var startIdx = type.IndexOf('(');
                var endIdx = type.IndexOf(')');
                if (startIdx >= 0 && endIdx > startIdx)
                {
                    return type.Substring(startIdx + 1, endIdx - startIdx - 1).Trim();
                }
                return string.Empty;
            }

            bool MemberMatchesEventCountry(Colaborador member, string countryKey)
            {
                if (member == null || string.IsNullOrWhiteSpace(member.Country) || string.IsNullOrWhiteSpace(countryKey))
                    return false;

                var memberCountry = member.Country.Trim().ToLowerInvariant();
                var key = countryKey.Trim().ToLowerInvariant();

                if (key == "eua" || key == "usa" || key == "united states" || key == "united states of america")
                {
                    return memberCountry.Contains("united states") || memberCountry.Contains("usa") || memberCountry.Contains("eua");
                }

                if (key == "brazil" || key == "brasil")
                {
                    return memberCountry.Contains("brazil") || memberCountry.Contains("brasil");
                }

                return memberCountry.Contains(key);
            }

            string GetIconClassByType(string type)
            {
                switch (type)
                {
                    case string s when s.Contains("Holiday", StringComparison.OrdinalIgnoreCase):
                        return "bi-calendar-check-fill";
                    case string s when s.Contains("Training", StringComparison.OrdinalIgnoreCase):
                        return "bi-lightbulb-fill";
                    default:
                        return "bi-calendar-event";
                }
            }

            foreach (var ev in holidayEvents)
            {
                var countryKey = ExtractCountryKey(ev.Type);

                // Contar quantos membros da sprint pertencem ao countryKey
                int affected = members.Count(m => MemberMatchesEventCountry(m, countryKey));

                if (affected == 0) continue;

                double affectedPercent = Math.Round((double)affected / totalMembers * 100, 1);
                string icon = "<span style='font-size: 22px; margin-right:6px;'>🔥</span>";
                string message = $"<span class='me-1'>{icon}</span> **{affectedPercent}% of the team** will be unavailable on **{ev.Date.ToString("MMM dd")}** due to a national holiday in {countryKey}.";


                notifications.Add(new SprintHolidayNotificationDto
                {
                    EventId = ev.Id,
                    EventTitle = ev.Title,
                    EventDate = ev.Date,
                    EventType = ev.Type,
                    IconClass = icon,
                    AffectedMembers = affected,
                    TotalMembers = totalMembers,
                    AffectedPercent = affectedPercent,
                    CountryKey = countryKey,
                    Message = message
                });
            }

            return Json(notifications);
        }



        // notificação ausencia 

        private int GetWorkingDays(DateTime start, DateTime end)
        {
            if (start > end) return 0;

            int workingDays = 0;
            DateTime date = start.Date;

            while (date <= end.Date)
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
                date = date.AddDays(1);
            }
            return workingDays;
        }

        // Verifica se um dia é útil
        private bool IsWorkingDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }



        [HttpGet]
        [Route("Sprint/GetAbsenceImpact")]
        public async Task<IActionResult> GetAbsenceImpact(int? sprintId = null)
        {
            var loggedUserName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(loggedUserName))
                return Unauthorized("User must be logged in.");

            var manager = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.FullName == loggedUserName);

            if (manager == null)
                return Unauthorized("User not found.");

            SprintModel sprint = sprintId.HasValue
                ? await _context.Sprints.FindAsync(sprintId.Value)
                : await _context.Sprints.FirstOrDefaultAsync(s => s.IsActive == true);

            if (sprint == null)
                return Json(new List<SprintAbsenceImpactDto>());

            // Buscar TODOS os membros da equipe do manager
            var teamMembers = await _context.Colaboradores
                .Where(c => c.TeamName == manager.TeamName)
                .ToListAsync();

            if (!teamMembers.Any())
                return Json(new List<SprintAbsenceImpactDto>());

            // Cálculo da capacidade total
            DateTime sprintStart = sprint.StartDate.Date;
            DateTime sprintEnd = sprint.EndDate.Date;
            const int HoursPerDay = 8;

            int totalWorkingDays = GetWorkingDays(sprintStart, sprintEnd);
            double totalAvailableHours = totalWorkingDays * teamMembers.Count * HoursPerDay;

            // Processar ausências
            var notifications = new List<SprintAbsenceImpactDto>();

            foreach (var m in teamMembers)
            {
                if (m.StartDate == null || m.EndDate == null)
                    continue;

                DateTime absenceStart = m.StartDate.Value.Date;
                DateTime absenceEnd = m.EndDate.Value.Date;

                if (absenceEnd < sprintStart || absenceStart > sprintEnd)
                    continue;

                DateTime overlapStart = absenceStart > sprintStart ? absenceStart : sprintStart;
                DateTime overlapEnd = absenceEnd < sprintEnd ? absenceEnd : sprintEnd;

                int absentWorkingDays = GetWorkingDays(overlapStart, overlapEnd);

                if (absentWorkingDays == 0)
                    continue;

                double absentHours = absentWorkingDays * HoursPerDay;
                double impactPercent = Math.Round((absentHours / totalAvailableHours) * 100, 1);

                string msg = $"{m.FullName} is absent until {overlapEnd:MMM dd}. Impact: {impactPercent}%.";

                notifications.Add(new SprintAbsenceImpactDto
                {
                    MemberId = m.Id,
                    MemberName = m.FullName,
                    Reason = m.InactivityReason,
                    AbsenceStart = m.StartDate,
                    AbsenceEnd = m.EndDate,
                    ImpactPercent = impactPercent,
                    AbsentDays = absentWorkingDays,
                    SprintDays = totalWorkingDays,
                    Message = msg
                });
            }

            return Json(notifications);
        }





        // --- Actions de Visualização Simples ---

        public IActionResult Rh()
        {
            ViewData["Title"] = "Sprint e Gantt do RH";
            ViewData["Profile"] = "Rh";
            return View(); 
        }


        [HttpGet]
        [Route("Sprint/GetAllTeams")]
        public async Task<IActionResult> GetAllTeams()
        {
            // Busca todos os nomes de equipe únicos (ignorando nulos ou vazios)
            var teams = await _context.Colaboradores
                .Where(c => c.TeamName != null && c.TeamName != "" &&
                            !c.TeamName.Contains("RH") &&
                            !c.TeamName.Contains("HR"))
                .Select(c => c.TeamName)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();

            return Json(teams.Select(t => new { TeamName = t }));
        }


[HttpGet]
[Route("Sprint/GetActiveSprintByTeam")]
public async Task<IActionResult> GetActiveSprintByTeam(string teamName)
{
    if (string.IsNullOrEmpty(teamName)) return BadRequest();

    // Ids da Equipe (Números)
    var teamMemberIds = await _context.Colaboradores
        .Where(c => c.TeamName == teamName)
        .Select(c => c.Id)
        .ToListAsync();

    if (!teamMemberIds.Any())
    {
        return NotFound(); 
    }

    // Busca todas as Sprints Ativas 
    var activeSprints = await _context.Sprints
        .Where(s => s.IsActive == true)
        .ToListAsync(); 

    var teamActiveSprint = activeSprints.FirstOrDefault(s =>
    {
        if (string.IsNullOrEmpty(s.ParticipatingMemberIds)) return false;

        try
        {
            var sprintParticipantIds = s.ParticipatingMemberIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(idStr => int.TryParse(idStr.Trim(), out int id) ? id : 0)
                .Where(id => id > 0)
                .ToHashSet(); 

            return teamMemberIds.Any(teamId => sprintParticipantIds.Contains(teamId));
        }
        catch (Exception ex)
        {
            // Se o split/parse falhar por dados ruins, logamos e ignoramos esta sprint.
            Console.WriteLine($"Erro ao processar IDs de sprint {s.Id}: {ex.Message}");
            return false;
        }
    });

    if (teamActiveSprint == null)
    {
        return NotFound(); 
    }

    return Json(teamActiveSprint);
}



        public IActionResult Colaborador()
        {
            ViewData["Title"] = "Minhas Sprints";
            ViewData["Profile"] = "Colaborador";
            return View(); 
        }

[HttpGet]
public IActionResult IsParticipatingAndGetTeamData()
{
    var loggedInUserIdentifier = User.FindFirst(ClaimTypes.Name)?.Value;

    if (string.IsNullOrEmpty(loggedInUserIdentifier))
        return Unauthorized(new { isParticipating = false });

    var user = _context.Colaboradores.FirstOrDefault(c => c.FullName == loggedInUserIdentifier);

    if (user == null)
        return NotFound(new { isParticipating = false });

    // PEGA SPRINT ATIVA
    var activeSprint = _context.Sprints.FirstOrDefault(s => s.IsActive == true);

    if (activeSprint == null)
        return Ok(new { isParticipating = false });

    bool isParticipating = false;

    if (!string.IsNullOrEmpty(activeSprint.ParticipatingMemberIds))
    {
        var ids = activeSprint.ParticipatingMemberIds
            .Split(',')
            .Select(id => int.Parse(id))
            .ToList();

        isParticipating = ids.Contains(user.Id);
    }

    // PEGA MEMBROS DA EQUIPE
    var teamMembersIds = _context.Colaboradores
        .Where(c => c.TeamName == user.TeamName)
        .Select(c => c.Id)
        .ToList();

    return Ok(new
    {
        isParticipating,
        sprintId = activeSprint.Id,
        teamMemberIds = teamMembersIds
    });
}


        //  Histórico filtrado para a equipe
        [HttpGet]
        [Route("Sprint/GetFinishedByTeam")]
        public async Task<IActionResult> GetFinishedByTeam([FromQuery] List<int> memberIds)
        {
            if (memberIds == null || !memberIds.Any())
            {
                return Json(new List<object>());
            }

            var finishedSprints = await _context.Sprints
                .Where(s => s.IsActive == false) 
                .Where(s => s.ParticipatingMemberIds != null && s.ParticipatingMemberIds != "")
                .ToListAsync();

            var relevantSprintIds = new HashSet<int>();

            foreach (var sprint in finishedSprints)
            {
                var sprintParticipantIds = sprint.ParticipatingMemberIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                    .Where(id => id > 0)
                    .ToList();

                if (sprintParticipantIds.Any(id => memberIds.Contains(id)))
                {
                    relevantSprintIds.Add(sprint.Id);
                }
            }

            var results = await _context.SprintResults
                .Where(r => relevantSprintIds.Contains(r.SprintId))
                .OrderByDescending(r => r.ActualFinishDate)
                .Take(20) 
                .ToListAsync();

            return Json(results);
        }

    }
}