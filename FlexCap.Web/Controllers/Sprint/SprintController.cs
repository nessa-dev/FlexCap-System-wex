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
    // A classe deve herdar de Controller
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
                // Caso não encontre o usuário
                return Json(new List<object>());
            }

            // 3️⃣ Identifica o nome da equipe do usuário
            var teamName = loggedUser.TeamName;

            // 4️⃣ Busca todos os colaboradores da mesma equipe
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

            // 5️⃣ Retorna os dados em formato JSON
            return Json(sameTeamMembers);
        }


        // EM SprintController.cs

        // EM SprintController.cs

        // EM SprintController.cs

        [HttpGet]
        public async Task<IActionResult> GetActive(int? id = null) // Mantemos 'id' para o fluxo do Colaborador
        {
            // 1. Obtém o usuário logado
            var loggedInUserName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(loggedInUserName)) return Unauthorized();

            var loggedUser = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.FullName == loggedInUserName);

            if (loggedUser == null) return NotFound();

            // 2. Caso de Uso: Busca por ID (Usado pelo Colaborador, se aplicável)
            if (id.HasValue && id.Value > 0)
            {
                var specificSprint = await _context.Sprints
                    .FirstOrDefaultAsync(s => s.Id == id.Value && s.IsActive == true);

                if (specificSprint != null) return Json(specificSprint);
                // Se a busca por ID falhar, continuamos buscando a sprint da equipe
            }

            // 3. Caso de Uso: Busca pela Sprint ATIVA da equipe

            // Primeiro, obtemos os IDs dos membros da equipe do usuário logado
            // (Assumimos que o time é definido pelo TeamName do Colaborador/Manager)
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

        // EM SprintController.cs

        [HttpPut]
        [Route("Sprint/Update")]
        public async Task<IActionResult> Update([FromBody] SprintModel updatedSprint)
        {
            if (updatedSprint.Id == 0 || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Busca a sprint original no banco de dados
            var originalSprint = await _context.Sprints.FindAsync(updatedSprint.Id);
            if (originalSprint == null) return NotFound();

            // 2. Atualiza apenas os campos permitidos
            originalSprint.Name = updatedSprint.Name;
            originalSprint.Goal = updatedSprint.Goal;
            originalSprint.StartDate = updatedSprint.StartDate;
            originalSprint.EndDate = updatedSprint.EndDate;
            originalSprint.Notes = updatedSprint.Notes;

            // Nota: Mantemos ParticipatingMemberIds se você não os mudou no modal de edição

            // 3. Salva
            _context.Sprints.Update(originalSprint);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 Success
        }

        // EM SprintController.cs

        [HttpDelete("Sprint/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sprintToDelete = await _context.Sprints.FindAsync(id);

            if (sprintToDelete == null) return NotFound();

            _context.Sprints.Remove(sprintToDelete);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 Success
        }


        // --- Endpoint para criar a Sprint ---
        // EM SprintController.cs

        [HttpPost]
        [Route("Sprint/CreateSprint")]
        public async Task<IActionResult> CreateSprint([FromBody] SprintModel model, [FromQuery] List<int> memberIds)
        {
            // 1. Validação do Estado do Modelo
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Validação simples de datas
            if (model.StartDate > model.EndDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
                return BadRequest(ModelState);
            }

            // --- 🔒 IMPLEMENTAÇÃO DA REGRA DE NEGÓCIO: SÓ PODE HAVER UMA ATIVA POR PARTICIPANTE ---

            // Converte os IDs de membro para strings para a checagem no banco de dados
            var memberIdStrings = memberIds.Select(id => id.ToString()).ToList();

            // 2. Checa se algum dos membros selecionados JÁ está em outra sprint ATIVA
            var conflictingActiveSprint = await _context.Sprints
                .Where(s => s.IsActive == true)
                .ToListAsync(); // Busca todas as ativas para fazer a checagem de string em memória

            var conflict = conflictingActiveSprint.FirstOrDefault(s =>
                memberIdStrings.Any(idStr => s.ParticipatingMemberIds.Contains(idStr)));

            if (conflict != null)
            {
                // 🛑 Falha na regra de negócio: Conflito de participante
                return StatusCode(409, $"A new sprint cannot be created. At least one selected member is already participating in active sprint '{conflict.Name}' (ID: {conflict.Id}).");
            }

            // --- FIM DA REGRA DE NEGÓCIO ---

            // 3. Configuração e Salvamento
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
            // 1. Validação de Estado
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 2. Salva o resultado da retrospectiva no banco (Histórico)
            _context.SprintResults.Add(result);

            // 3. Marca a sprint original como inativa/completa
            var sprint = await _context.Sprints.FindAsync(result.SprintId);

            if (sprint != null)
            {
                // 💡 CORREÇÃO: Usando a propriedade IsActive ou IsCompleted
                sprint.IsActive = false; // Se você adicionou 'IsActive', use false.
                                         // OU sprint.IsCompleted = true; // Se você adicionou 'IsCompleted', use true.

                // O Entity Framework rastreia a mudança, mas chamar Update é uma boa prática
                // se o objeto não foi rastreado em outro lugar.
                _context.Sprints.Update(sprint);
            }

            // 4. Salva as mudanças (SprintResult e Sprint status)
            await _context.SaveChangesAsync();

            // Retorna 204 No Content (padrão para sucesso em POST/PUT que não retorna objeto)
            return NoContent();
        }



        [HttpGet]
        [Route("Sprint/GetFinished")]
        public async Task<IActionResult> GetFinished()
        {
            // 1. Obtém o usuário logado (Manager)
            var loggedInUserName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(loggedInUserName)) return Unauthorized();

            var loggedUser = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.FullName == loggedInUserName);

            if (loggedUser == null) return NotFound("User not found.");

            // 2. Identifica todos os membros da equipe do Manager logado
            var teamMemberIds = await _context.Colaboradores
                .Where(c => c.TeamName == loggedUser.TeamName) // Filtra pelo nome da equipe
                .Select(c => c.Id)
                .ToListAsync();

            var teamMemberIdStrings = teamMemberIds.Select(i => i.ToString()).ToList();

            // 3. Busca todas as Sprints que JÁ FORAM FINALIZADAS (IsActive == false)
            var finishedSprints = await _context.Sprints
                .Where(s => s.IsActive == false)
                .ToListAsync();

            var relevantSprintIds = new HashSet<int>();

            // 4. CRÍTICO: Filtra as Sprints que pertencem à equipe do Manager logado
            foreach (var sprint in finishedSprints)
            {
                // Checa se a string de participantes da sprint finalizada contém o ID de alguém da equipe
                if (teamMemberIdStrings.Any(idStr => sprint.ParticipatingMemberIds != null && sprint.ParticipatingMemberIds.Contains(idStr)))
                {
                    relevantSprintIds.Add(sprint.Id);
                }
            }

            // 5. Busca os resultados de retrospectiva (SprintResults) APENAS para as Sprints relevantes
            if (!relevantSprintIds.Any())
            {
                return Json(new List<SprintResultModel>()); // Retorna vazio se não houver histórico para esta equipe
            }

            var results = await _context.SprintResults
                .Where(r => relevantSprintIds.Contains(r.SprintId))
                .OrderByDescending(r => r.ActualFinishDate)
                .Take(50) // Mantém o limite de segurança
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
                // Retorna JSON vazio se não houver sprint ativa
                return Json(new List<SprintHolidayNotificationDto>());
            }

            // 2) Identificar membros participantes
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

            // Se não há IDs na sprint, busca todos os membros da equipe (fallback do seu código)
            List<Colaborador> members;
            if (memberIds.Count > 0)
            {
                members = await _context.Colaboradores
                    .Where(c => memberIds.Contains(c.Id))
                    .ToListAsync();
            }
            else
            {
                // Se a sprint não tem IDs, este fallback é necessário para evitar 0 membros
                members = await _context.Colaboradores.ToListAsync();
            }

            int totalMembers = members.Count;

            // Se a equipe não tem membros, não há impacto a ser calculado
            if (totalMembers == 0)
            {
                return Json(new List<SprintHolidayNotificationDto>());
            }

            // 3) Pegar eventos do calendário que caem dentro do período da sprint
            var start = sprint.StartDate.Date;
            var end = sprint.EndDate.Date;

            var events = await _context.CalendarEvents // 💡 Assume DbSet<CalendarEvents> no AppDbContext
                .Where(e => e.Date.Date >= start && e.Date.Date <= end)
                .OrderBy(e => e.Date)
                .ToListAsync();

            var holidayEvents = events
                .Where(e => !string.IsNullOrWhiteSpace(e.Type) && e.Type.StartsWith("Holiday", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var notifications = new List<SprintHolidayNotificationDto>();

            // ----------------------------------------------------------------------------------
            // FUNÇÕES UTILIÁRIAS LOCAIS (para evitar erros de escopo e garantir a compilação)
            // ----------------------------------------------------------------------------------

            // Utilitário para extrair o País/Chave do título do evento (Ex: 'Brazil' de 'Holiday (Brazil)')
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

            // Utilitário para checar se o Colaborador é afetado pelo país/chave
            bool MemberMatchesEventCountry(Colaborador member, string countryKey)
            {
                if (member == null || string.IsNullOrWhiteSpace(member.Country) || string.IsNullOrWhiteSpace(countryKey))
                    return false;

                var memberCountry = member.Country.Trim().ToLowerInvariant();
                var key = countryKey.Trim().ToLowerInvariant();

                // Esta é a lógica de normalização complexa (sinônimos) que você já tinha:
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

            // ----------------------------------------------------------------------------------


            foreach (var ev in holidayEvents)
            {
                var countryKey = ExtractCountryKey(ev.Type);

                // Contar quantos membros da sprint pertencem a esse countryKey
                int affected = members.Count(m => MemberMatchesEventCountry(m, countryKey));

                if (affected == 0) continue;

                // 💡 CÁLCULO FINAL: Porcentagem de MEMBROS AFETADOS
                double affectedPercent = Math.Round((double)affected / totalMembers * 100, 1);

                // Monta a mensagem final:
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
                    // 💡 NOVO: Mensagem formatada
                    Message = message
                });
            }

            // 💡 IMPORTANTE: Você precisa garantir que o DTO SprintHolidayNotificationDto tenha o campo Message.

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
                // Ignora Sábado (DayOfWeek.Saturday) e Domingo (DayOfWeek.Sunday)
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
                date = date.AddDays(1);
            }
            return workingDays;
        }

        // 2. Verifica se um dia é útil
        private bool IsWorkingDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }


        [HttpGet]
        [Route("Sprint/GetAbsenceImpact")]
        public async Task<IActionResult> GetAbsenceImpact(int? sprintId = null)
        {
            // 1. Usuário logado
            var loggedUserName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(loggedUserName))
                return Unauthorized("User must be logged in.");

            var manager = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.FullName == loggedUserName);

            if (manager == null)
                return Unauthorized("User not found.");

            // 2. Sprint ativa
            SprintModel sprint = sprintId.HasValue
                ? await _context.Sprints.FindAsync(sprintId.Value)
                : await _context.Sprints.FirstOrDefaultAsync(s => s.IsActive == true);

            if (sprint == null)
                return Json(new List<SprintAbsenceImpactDto>());

            // 3. Buscar TODOS os membros da equipe do manager
            var teamMembers = await _context.Colaboradores
                .Where(c => c.TeamName == manager.TeamName)
                .ToListAsync();

            if (!teamMembers.Any())
                return Json(new List<SprintAbsenceImpactDto>());

            // 4. Cálculo da capacidade total
            DateTime sprintStart = sprint.StartDate.Date;
            DateTime sprintEnd = sprint.EndDate.Date;
            const int HoursPerDay = 8;

            int totalWorkingDays = GetWorkingDays(sprintStart, sprintEnd);
            double totalAvailableHours = totalWorkingDays * teamMembers.Count * HoursPerDay;

            // 5. Processar ausências
            var notifications = new List<SprintAbsenceImpactDto>();

            foreach (var m in teamMembers)
            {
                // Se não tiver datas, não está ausente
                if (m.StartDate == null || m.EndDate == null)
                    continue;

                DateTime absenceStart = m.StartDate.Value.Date;
                DateTime absenceEnd = m.EndDate.Value.Date;

                // Se não cruzar com a sprint, ignora
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
            return View(); // Retorna Views/Sprint/Rh.cshtml
        }

        // Retorna a lista de todas as equipes (nomes únicos) para popular o `<select>`.

        // EM SprintController.cs

        [HttpGet]
        [Route("Sprint/GetAllTeams")]
        public async Task<IActionResult> GetAllTeams()
        {
            // Busca todos os nomes de equipe únicos (ignorando nulos ou vazios)
            var teams = await _context.Colaboradores
                // 💡 CRÍTICO: Filtra para garantir que o nome do time não seja nulo, vazio, 
                // e que não contenha "RH" ou "HR" (case-insensitive para segurança).
                .Where(c => c.TeamName != null && c.TeamName != "" &&
                            !c.TeamName.Contains("RH") &&
                            !c.TeamName.Contains("HR"))
                .Select(c => c.TeamName)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();

            // Retorna a lista de nomes de equipe em um formato JSON amigável
            return Json(teams.Select(t => new { TeamName = t }));
        }


        // Busca a sprint ativa que pertence à equipe selecionada.

        // EM SprintController.cs

        // EM SprintController.cs

        [HttpGet]
        [Route("Sprint/GetActiveSprintByTeam")]
        public async Task<IActionResult> GetActiveSprintByTeam(string teamName)
        {
            if (string.IsNullOrEmpty(teamName)) return BadRequest();

            // 1. Ids da Equipe (Números)
            var teamMemberIds = await _context.Colaboradores
                .Where(c => c.TeamName == teamName)
                .Select(c => c.Id)
                .ToListAsync();

            if (!teamMemberIds.Any())
            {
                return NotFound();
            }

            // 2. Busca todas as Sprints Ativas (para filtrar localmente)
            var activeSprints = await _context.Sprints
                .Where(s => s.IsActive == true)
                .ToListAsync();

            // 3. CRÍTICO: Filtragem por conversão rigorosa
            var teamActiveSprint = activeSprints.FirstOrDefault(s =>
            {
                if (string.IsNullOrEmpty(s.ParticipatingMemberIds)) return false;

                try
                {
                    // Converte a string do banco (ex: "1,2,10") em uma lista de inteiros {1, 2, 10}
                    var sprintParticipantIds = s.ParticipatingMemberIds
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(idStr => int.TryParse(idStr.Trim(), out int id) ? id : 0)
                        .Where(id => id > 0)
                        .ToHashSet(); // Usa HashSet para busca rápida

                    // Verifica se ALGUM dos IDs da equipe selecionada (teamMemberIds)
                    // está presente no conjunto de IDs da sprint (sprintParticipantIds).
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
            return View(); // Retorna Views/Sprint/Colaborador.cshtml
        }


        // EM SprintController.cs

        // ----------------------------------------------------------
// NOVO ENDPOINT NECESSÁRIO PARA A VIEW DO COLABORADOR FUNCIONAR
// ----------------------------------------------------------
[HttpGet]
public IActionResult IsParticipatingAndGetTeamData()
{
    // PEGA O USUÁRIO LOGADO
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

    // PARTICIPAÇÃO?
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


        // EM SprintController.cs

        // 🎯 NOVO ENDPOINT: Histórico filtrado para a equipe
        [HttpGet]
        [Route("Sprint/GetFinishedByTeam")]
        public async Task<IActionResult> GetFinishedByTeam([FromQuery] List<int> memberIds)
        {
            // Se nenhum ID de membro for fornecido, retorna vazio (segurança)
            if (memberIds == null || !memberIds.Any())
            {
                return Json(new List<object>());
            }

            // 1. Encontrar todos os IDs de sprints que têm resultados e cujo ID
            // está na lista de IDs de membros participantes (ParticipatingMemberIds)
            var finishedSprints = await _context.Sprints
                .Where(s => s.IsActive == false) // Garante que foi finalizado
                .Where(s => s.ParticipatingMemberIds != null && s.ParticipatingMemberIds != "")
                .ToListAsync();

            var relevantSprintIds = new HashSet<int>();

            // Filtra as Sprints onde pelo menos um dos 'memberIds' está na lista de participantes da Sprint
            foreach (var sprint in finishedSprints)
            {
                var sprintParticipantIds = sprint.ParticipatingMemberIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                    .Where(id => id > 0)
                    .ToList();

                // Checa se há intersecção entre os IDs da equipe e os participantes do sprint
                if (sprintParticipantIds.Any(id => memberIds.Contains(id)))
                {
                    relevantSprintIds.Add(sprint.Id);
                }
            }

            // 2. Buscar os resultados de retrospectiva (SprintResults) para as Sprints relevantes
            var results = await _context.SprintResults
                .Where(r => relevantSprintIds.Contains(r.SprintId))
                .OrderByDescending(r => r.ActualFinishDate)
                .Take(20) // Exibe os 20 mais recentes
                .ToListAsync();

            // 3. Retorna os resultados do histórico (apenas da equipe)
            return Json(results);
        }




    }
}