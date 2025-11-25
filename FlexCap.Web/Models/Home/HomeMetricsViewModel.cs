using FlexCap.Web.Models.Requests;
using System.Collections.Generic;

namespace FlexCap.Web.Models
{
    public class HomeMetricsViewModel
    {
        public int ActiveSprintCount { get; set; }

        // --- PROPRIEDADES DA SPRINT ATIVA (ADICIONADAS) ---
        public bool IsSprintActive { get; set; } // Flag: true se houver uma sprint ativa para o time
        public string SprintName { get; set; } // Nome da sprint ativa
        public string SprintDuration { get; set; } // Duração formatada (Ex: "Jun 01 - Jun 25")
        public double SprintProgressPercent { get; set; } // Progresso calculado (0.0 a 100.0)
        public List<string> SprintImpactNotifications { get; set; } = new List<string>(); // Mensagens de impacto/ausência
        // ----------------------------------------------------

        // Propriedades do RH (Mantidas)
        public int TotalPendingHR { get; set; }
        public int TotalEmployees { get; set; }
        public int ActiveSectorsCount { get; set; }

        // Propriedades de Requisições
        public List<PendingRequestListItemViewModel> LatestPendingRequests { get; set; } = new List<PendingRequestListItemViewModel>();

        // 🛑 NOVAS PROPRIEDADES DE PERFIL (Para substituir ViewData)
        public string FirstName { get; set; } = "Usuário";
        public string UserTeam { get; set; } = "Sem Time";
        public int ActiveMembers { get; set; }
        public int TotalMembers { get; set; }

        // 🛑 Propriedade para o Histórico do Colaborador (Os 3 itens recentes)
        public List<PendingRequestListItemViewModel> ColaboratorHistory { get; set; } = new List<PendingRequestListItemViewModel>();
        public List<PendingRequestListItemViewModel> PendingManagerRequests { get; set; } = new List<PendingRequestListItemViewModel>();
    }
}