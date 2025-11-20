using FlexCap.Web.Data;
using FlexCap.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System; 
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FlexCap.Web.Models.Sprint;

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

        [HttpGet]
        public async Task<IActionResult> GetActive()
        {
            // 💡 CORREÇÃO: Deve buscar APENAS sprints marcadas como IsActive = true
            var activeSprint = await _context.Sprints
                .OrderByDescending(s => s.EndDate)
                // Adiciona a checagem da propriedade IsActive
                .FirstOrDefaultAsync(s => s.IsActive == true);

            if (activeSprint == null)
            {
                // Se a sprint foi marcada como Inativa (IsActive=false), este retorno 404
                // diz ao frontend que não há sprint para exibir.
                return NotFound();
            }

            return Json(activeSprint);
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
        [HttpPost]
        [Route("Sprint/CreateSprint")]
        public async Task<IActionResult> CreateSprint([FromBody] SprintModel model, [FromQuery] List<int> memberIds)
        {
            // 1. Validação do Estado do Modelo
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- 🔒 IMPLEMENTAÇÃO DA REGRA DE NEGÓCIO: SÓ PODE HAVER UMA ATIVA ---

            // 2. Checa se já existe uma sprint ATIVA no banco de dados.
            var existingActiveSprint = await _context.Sprints
                // Busca a primeira sprint onde IsActive é TRUE
                .FirstOrDefaultAsync(s => s.IsActive == true);

            if (existingActiveSprint != null)
            {
                // 🛑 Falha na regra de negócio. Retorna um status HTTP 409 Conflict 
                // ou 400 Bad Request com uma mensagem clara.
                return StatusCode(409, "A new sprint cannot be created. There is already an active sprint running. Please finish the current sprint first.");
            }

            // --- FIM DA REGRA DE NEGÓCIO ---

            // 3. Validação simples de datas
            if (model.StartDate > model.EndDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
                return BadRequest(ModelState);
            }

            // 4. Configuração do Modelo

            // Mapeia os IDs dos membros em uma string para salvar
            model.ParticipatingMemberIds = string.Join(",", memberIds.Distinct());

            // 💡 CRÍTICO: Marca a nova sprint como ATIVA.
            model.IsActive = true;

            // 5. Salva no Banco de Dados
            _context.Sprints.Add(model);
            await _context.SaveChangesAsync();

            // Retorna o objeto criado para o frontend (para fins de atualização de UI)
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
            var results = await _context.SprintResults
                .OrderByDescending(r => r.ActualFinishDate)
                .Take(50) // limite de segurança
                .ToListAsync();

            return Json(results);
        }



        // --- Actions de Visualização Simples ---

        public IActionResult Rh()
        {
            ViewData["Title"] = "Sprint e Gantt do RH";
            ViewData["Profile"] = "Rh";
            return View(); // Retorna Views/Sprint/Rh.cshtml
        }

        public IActionResult Colaborador()
        {
            ViewData["Title"] = "Minhas Sprints";
            ViewData["Profile"] = "Colaborador";
            return View(); // Retorna Views/Sprint/Colaborador.cshtml
        }

    }
}