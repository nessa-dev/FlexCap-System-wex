let selectedMemberIds = [];
let teamMembers = []; // Será preenchido via AJAX
const totalDays = 30; // Número de colunas no grid

document.addEventListener('DOMContentLoaded', function () {
    const headerEl = document.getElementById('gantt-header');
    const containerEl = document.getElementById('gantt-chart-container');
    const membersListContainer = document.getElementById('membersListContainer');
    const selectAllBtn = document.getElementById('selectAllMembers');
    const clearSelectionBtn = document.getElementById('clearSelection');
    const saveSprintBtn = document.getElementById('saveSprintBtn');

    // Modais e botões de navegação
    const registerSprintModalEl = document.getElementById('registerSprintModal');
    const membersModalEl = document.getElementById('membersModal');
    const backToRegisterModalBtn = document.getElementById('backToRegisterModalBtn');
    const doneSelectingMembersBtn = document.getElementById('doneSelectingMembersBtn');
    const selectMembersBtn = document.getElementById('selectMembersBtn');

    // --- FUNÇÃO CRÍTICA DE NAVEGAÇÃO ASSÍNCRONA (corrigida) ---
    function hideModalAndShowParent(hideModalEl, showModalEl) {
        const hideModalInstance = bootstrap.Modal.getInstance(hideModalEl);

        function cleanBackdrops() {
            // remove backdrops extras e garante body limpo
            const backdrops = document.querySelectorAll('.modal-backdrop');
            if (backdrops.length) {
                backdrops.forEach(b => b.remove());
            }
            document.body.classList.remove('modal-open');
            document.body.style.removeProperty('overflow');
            document.body.style.removeProperty('padding-right');
        }

        if (hideModalInstance) {
            hideModalEl.addEventListener('hidden.bs.modal', function handler() {
                hideModalEl.removeEventListener('hidden.bs.modal', handler);
                // leve delay para garantir que bootstrap terminou a animação
                setTimeout(() => {
                    cleanBackdrops();
                    const showModalInstance = new bootstrap.Modal(showModalEl);
                    showModalInstance.show();
                }, 50);
            }, { once: true });

            hideModalInstance.hide();
        } else {
            // Se já estiver fechado, limpa e mostra direto
            cleanBackdrops();
            const showModalInstance = new bootstrap.Modal(showModalEl);
            showModalInstance.show();
        }
    }

    // --- HANDLER GLOBAL DE LIMPEZA (protege contra backdrops sobrando) ---
    document.addEventListener('hidden.bs.modal', function () {
        const backdrops = document.querySelectorAll('.modal-backdrop');
        if (backdrops.length > 0) {
            // remove todos os backdrops (segurança)
            backdrops.forEach(b => b.remove());
        }
        document.body.classList.remove('modal-open');
        document.body.style.removeProperty('overflow');
        document.body.style.removeProperty('padding-right');
    });

    // --- LÓGICA AJAX: CARREGAMENTO DOS MEMBROS DA EQUIPE (corrigido) ---
    async function loadTeamMembers() {
        membersListContainer.innerHTML = '<p class="text-info small text-center"><i class="bi bi-arrow-clockwise spin"></i> Carregando membros da equipe...</p>';

        try {
            const response = await fetch('/Sprint/GetTeamMembers');

            if (!response.ok) {
                if (response.status === 401) {
                    membersListContainer.innerHTML = `<p class="text-danger small">Sessão expirada. Faça login novamente.</p>`;
                    return;
                }
                throw new Error(`Erro ao buscar membros: ${response.statusText}`);
            }

            const data = await response.json();

            // FIX: usa corretamente 'name' ou 'fullName' conforme o JSON
            teamMembers = data.map(m => ({
                id: m.id,
                name: m.name || m.fullName || '',      // aceita 'name' ou 'fullName'
                position: m.position || '',
                photoUrl: m.photoUrl || '',
                status: m.status || ''
            }));

            // Se quiser marcar todos por padrão:
            selectedMemberIds = teamMembers.map(m => m.id);

            renderMembersList();

        } catch (error) {
            console.error("Erro no carregamento AJAX dos membros:", error);
            membersListContainer.innerHTML = `<p class="text-danger small">Erro ao carregar membros. Verifique o console.</p>`;
        }
    }

    // --- LÓGICA DE RENDERIZAÇÃO DOS MEMBROS ---
    function renderMembersList() {
        membersListContainer.innerHTML = '';

        if (!teamMembers || teamMembers.length === 0) {
            membersListContainer.innerHTML = '<p class="text-muted small">Nenhum membro encontrado nesta equipe.</p>';
            return;
        }

        teamMembers.forEach(member => {
            const isChecked = selectedMemberIds.includes(member.id);
            const item = document.createElement('div');
            item.className = 'list-group-item list-group-item-action d-flex align-items-center';
            item.innerHTML = `
                    <input class="form-check-input me-3" type="checkbox" value="${member.id}" id="member-${member.id}" ${isChecked ? 'checked' : ''}>
                    <label class="form-check-label flex-grow-1" for="member-${member.id}">
                        ${member.name || '<span class="text-muted small">(sem nome)</span>'}
                        <small class="text-muted"> ${member.position ? '(' + member.position + ')' : ''}</small>
                    </label>
                `;

            const checkbox = item.querySelector('input');
            checkbox.addEventListener('change', (e) => {
                const id = parseInt(e.target.value);
                if (e.target.checked) {
                    if (!selectedMemberIds.includes(id)) selectedMemberIds.push(id);
                } else {
                    selectedMemberIds = selectedMemberIds.filter(mid => mid !== id);
                }
            });

            membersListContainer.appendChild(item);
        });
    }

    // Lógica de Seleção Rápida
    if (selectAllBtn) {
        selectAllBtn.addEventListener('click', () => {
            selectedMemberIds = teamMembers.map(m => m.id);
            membersListContainer.querySelectorAll('input[type="checkbox"]').forEach(checkbox => {
                checkbox.checked = true;
            });
        });
    }

    if (clearSelectionBtn) {
        clearSelectionBtn.addEventListener('click', () => {
            selectedMemberIds = [];
            membersListContainer.querySelectorAll('input[type="checkbox"]').forEach(checkbox => {
                checkbox.checked = false;
            });
        });
    }

    // --- LÓGICA DO GRÁFICO DE GANTT (mantida) ---
    function generateHeader() {
        let html = '';
        let currentMonth = '';
        let monthStart = 1;

        for (let i = 0; i < totalDays; i++) {
            let date = new Date(2025, 5, 3 + i);
            let dayOfMonth = date.getDate();
            let monthName = date.toLocaleString('en-US', { month: 'short' });

            if (monthName !== currentMonth) {
                if (currentMonth) {
                    html = html.replace(`month-start-${monthStart}`, `grid-column: ${monthStart} / ${i + 1};`);
                }
                currentMonth = monthName;
                monthStart = i + 1;
                html += `<div class="gantt-month gantt-cell" id="month-start-${monthStart}">${monthName}</div>`;
            }
            html += `<div class="gantt-cell">${dayOfMonth}</div>`;
        }

        html = html.replace(`month-start-${monthStart}`, `grid-column: ${monthStart} / ${totalDays + 1};`);
        html += `<div class="gantt-cell">-</div>`;

        headerEl.innerHTML = html;
        containerEl.style.gridTemplateColumns = `repeat(${totalDays + 1}, 1fr)`;
    }

    function positionBars() {
        document.querySelectorAll('.gantt-bar').forEach(bar => {
            const startDay = parseInt(bar.dataset.start);
            const endDay = parseInt(bar.dataset.end);
            bar.style.gridColumn = `${startDay + 1} / ${endDay + 2}`;
        });
    }

    function setupTooltips() {
        document.querySelectorAll('.gantt-bar').forEach(bar => {
            const isAbsence = bar.classList.contains('absence-bar');
            const tooltipContent = bar.dataset.tooltipContent;
            const tooltipText = isAbsence ? tooltipContent : bar.title;

            if (!tooltipText) return;

            let tooltipEl;

            bar.addEventListener('mouseenter', () => {
                tooltipEl = document.createElement('div');
                tooltipEl.className = 'gantt-tooltip';
                tooltipEl.textContent = tooltipText;

                if (isAbsence) {
                    tooltipEl.style.backgroundColor = '#fff6da';
                    tooltipEl.style.borderColor = '#f9d774';
                } else {
                    tooltipEl.style.backgroundColor = '#343a40';
                    tooltipEl.style.color = '#ffffff';
                }

                document.body.appendChild(tooltipEl);
                updateTooltipPosition(bar, tooltipEl);
            });

            bar.addEventListener('mousemove', () => {
                if (tooltipEl) updateTooltipPosition(bar, tooltipEl);
            });

            bar.addEventListener('mouseleave', () => {
                if (tooltipEl) tooltipEl.remove();
                tooltipEl = null;
            });
        });
    }

    function updateTooltipPosition(targetEl, tooltipEl) {
        const rect = targetEl.getBoundingClientRect();
        let x = rect.left + rect.width / 2;
        let y = rect.bottom;
        tooltipEl.style.left = `${x}px`;
        tooltipEl.style.top = `${y}px`;
    }

    // --- MANUSEIO DA NAVEGAÇÃO E SUBMISSÃO ---

    if (saveSprintBtn) {
        saveSprintBtn.addEventListener('click', async () => {
            const sprintData = {
                Name: document.getElementById('sprintName').value,
                Goal: document.getElementById('sprintGoal').value,
                StartDate: document.getElementById('sprintStartDate').value,
                EndDate: document.getElementById('sprintEndDate').value,
                Notes: document.getElementById('sprintNotes').value,
            };

            if (!sprintData.Name || !sprintData.StartDate || !sprintData.EndDate || selectedMemberIds.length === 0) {
                alert("Please fill in the name, dates, and select at least one member.");
                return;
            }

            const memberQuery = selectedMemberIds.map(id => `memberIds=${id}`).join('&');
            const url = `/Sprint/CreateSprint?${memberQuery}`;

            try {
                const resp = await fetch(url, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(sprintData)
                });

                if (resp.ok) {
                    // 🟢 SUCESSO
                    const savedSprint = await resp.json();
                    alert(`Sprint '${savedSprint.Name}' successfully created!`);

                    document.getElementById('createSprintForm').reset();
                    const modal = bootstrap.Modal.getInstance(registerSprintModalEl);
                    if (modal) modal.hide();

                    // Recarrega a página para atualizar o Gantt, o Histórico e o status do botão "Criar Sprint".
                    window.location.reload();

                } else if (resp.status === 409) {
                    // 🛑 TRATAMENTO ESPECÍFICO DA REGRA DE NEGÓCIO (Conflito: Já existe uma ativa)
                    const errorText = await resp.text();
                    console.error("Server Conflict Response:", errorText);
                    alert(`Atenção: ${errorText}`);

                } else {
                    // 🛑 ERROS GERAIS (Ex: 400 Bad Request por falha de validação de ModelState/Datas)
                    const errorText = await resp.text();
                    console.error("Server Response Error:", errorText);
                    // Tenta parsear o JSON para dar um feedback melhor, se for 400
                    try {
                        const errorJson = JSON.parse(errorText);
                        alert(`Falha na criação da sprint: ${JSON.stringify(errorJson)}`);
                    } catch {
                        alert(`Falha ao salvar sprint (Status: ${resp.status}). Verifique o console.`);
                    }
                }

            } catch (err) {
                console.error("Error creating sprint:", err);
                alert("An error occurred while saving the sprint. See console for details.");
            }
        });
    }

    // 1. Botão "Back" (Modal de Membros -> Modal de Registro)
    if (backToRegisterModalBtn) {
        backToRegisterModalBtn.addEventListener('click', () => {
            hideModalAndShowParent(membersModalEl, registerSprintModalEl);
        });
    }

    // 2. Botão "Done" (Modal de Membros -> Modal de Registro)
    if (doneSelectingMembersBtn) {
        doneSelectingMembersBtn.addEventListener('click', () => {
            // Aqui você pode passar os selectedMemberIds pro modal pai antes de abrir
            hideModalAndShowParent(membersModalEl, registerSprintModalEl);
        });
    }

    // 3. Botão "Members" (Modal de Registro -> Modal de Membros)
    if (selectMembersBtn) {
        selectMembersBtn.addEventListener('click', async (e) => {
            e.preventDefault();
            // Carrega os membros antes de trocar de modal (espera terminar)
            await loadTeamMembers();
            hideModalAndShowParent(registerSprintModalEl, membersModalEl);
        });
    }

    // --- INICIA O GANTT NA CARGA DA PÁGINA ---
    generateHeader();
    positionBars();
    setupTooltips();
});

// --- edição ---


document.addEventListener('DOMContentLoaded', () => {
    // 1. Elementos do Modal e Formulário
    const editModalEl = document.getElementById('editSprintModal');
    // Adicione um container para mensagens/loading dentro do modal:
    const loadingEl = editModalEl ? editModalEl.querySelector('#editSprintLoading') : null;
    const formEl = editModalEl ? editModalEl.querySelector('#editSprintForm') : null;

    // Certifique-se de que esses IDs existem no seu modal de edição
    const deleteBtn = document.getElementById('deleteSprintBtn');
    const saveBtn = document.getElementById('saveSprintChangesBtn');

    // Campos de Input
    const nameInput = document.getElementById('editSprintName');
    const goalInput = document.getElementById('editSprintGoal');
    const startInput = document.getElementById('editSprintStartDate');
    const endInput = document.getElementById('editSprintEndDate');
    const notesInput = document.getElementById('editSprintNotes');

    let currentSprint = null; // Armazena os dados da Sprint carregada

    // Verifica se os elementos críticos existem antes de anexar listeners
    if (!editModalEl || !loadingEl || !formEl || !deleteBtn || !saveBtn) {
        console.error("Missing critical element IDs for Edit Sprint Modal.");
        return;
    }

    // --- Função para carregar sprint ativa ---
    async function loadActiveSprint() {
        // Estado de Carregamento
        loadingEl.classList.remove('d-none');
        loadingEl.innerHTML = `<p class="text-info">Loading active sprint...</p>`;
        formEl.classList.add('d-none');
        deleteBtn.classList.add('d-none');
        saveBtn.classList.add('d-none');
        currentSprint = null;

        try {
            // CRÍTICO: Chama o endpoint GET para a sprint ativa
            const resp = await fetch('/Sprint/GetActive');

            if (!resp.ok) {
                if (resp.status === 404) {
                    loadingEl.innerHTML = `<p class="text-muted">No active sprint found.</p>`;
                    return;
                }
                throw new Error(`Erro: ${resp.status}`);
            }

            const sprint = await resp.json();
            currentSprint = sprint;

            // Preenche os campos (Assumindo camelCase no JSON do C#)
            nameInput.value = sprint.name || '';
            goalInput.value = sprint.goal || '';

            // Datas vêm no formato ISO 8601 (ex: "2025-11-12T00:00:00"), precisamos apenas da data ("2025-11-12")
            startInput.value = sprint.startDate ? sprint.startDate.split('T')[0] : '';
            endInput.value = sprint.endDate ? sprint.endDate.split('T')[0] : '';

            notesInput.value = sprint.notes || '';

            // Exibe o formulário
            loadingEl.classList.add('d-none');
            formEl.classList.remove('d-none');
            deleteBtn.classList.remove('d-none');
            saveBtn.classList.remove('d-none');
        } catch (err) {
            console.error('Erro ao carregar sprint ativa:', err);
            loadingEl.innerHTML = `<p class="text-danger">Error loading active sprint.</p>`;
        }
    }

    // --- Salvar alterações (PUT) ---
    saveBtn.addEventListener('click', async () => {
        if (!currentSprint) return alert('No active sprint to edit.');

        const updated = {
            // CRÍTICO: Envia o ID original para saber qual sprint atualizar
            id: currentSprint.id,
            name: nameInput.value,
            goal: goalInput.value,
            startDate: startInput.value, // Deve ser YYYY-MM-DD
            endDate: endInput.value,    // Deve ser YYYY-MM-DD
            notes: notesInput.value,
            participatingMemberIds: currentSprint.participatingMemberIds
        };

        if (!updated.name || !updated.startDate || !updated.endDate) {
            return alert('Please fill in the Name and date range.');
        }

        try {
            const resp = await fetch('/Sprint/Update', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(updated)
            });

            // 💡 CORREÇÃO CRÍTICA: Trata tanto 200 OK quanto 204 No Content como sucesso.
            if (resp.ok || resp.status === 204) {

                // 1. Fecha o modal
                bootstrap.Modal.getInstance(editModalEl).hide();

                // 2. Avisa o usuário
                alert('Sprint updated successfully! The page will now refresh.');

                // 💡 SOLUÇÃO FINAL: Força o recarregamento da página. 
                // Isso garante que o Gantt, notificações e todos os dados sejam atualizados.
                window.location.reload();

            } else {
                // Se houver um erro real (4xx ou 5xx), tentamos ler o corpo da resposta
                let errorMessage = `Error saving sprint (Status: ${resp.status}).`;
                try {
                    // Tenta ler JSON para feedback detalhado
                    const errorBody = await resp.json();
                    errorMessage += ` Details: ${JSON.stringify(errorBody)}`;
                } catch {
                    // Se não for JSON, apenas retorna o status
                    errorMessage += ` Check server logs.`;
                }

                console.error('Update Error Details:', errorMessage);
                alert(errorMessage);
            }
        } catch (err) {
            console.error("Critical connection or execution error:", err);
            alert('Error updating sprint: A critical error occurred. See console for details.');
        }
    })


    // --- Deletar sprint (DELETE) ---
    deleteBtn.addEventListener('click', async () => {
        if (!currentSprint) return;
        if (!confirm(`Are you sure you want to delete "${currentSprint.name}" (ID: ${currentSprint.id})? This action cannot be undone.`)) return;

        try {
            const resp = await fetch(`/Sprint/Delete/${currentSprint.id}`, { method: 'DELETE' });

            // 💡 CORREÇÃO 1: Trata tanto 200 OK quanto 204 No Content como sucesso.
            // Isso impede que o fluxo caia no 'else' se o servidor retornar 204.
            if (resp.ok || resp.status === 204) {

                // 1. Fecha o modal (importante fazer isso antes da recarga)
                const modalInstance = bootstrap.Modal.getInstance(editModalEl);
                if (modalInstance) {
                    modalInstance.hide();
                }

                // 2. Avisa o usuário
                alert('Sprint deleted successfully!');

                // 💡 CORREÇÃO 2: Força o recarregamento da página. 
                // Esta é a forma mais robusta e completa de garantir que o Gantt e 
                // todos os botões (ex: "Criar Sprint") atualizem corretamente.
                // Isso interrompe o JS, evitando que o bloco 'catch' seja acionado depois.
                window.location.reload();

            } else {
                // Se falhar (e o status for 4xx ou 5xx)
                const errorText = await resp.text();
                console.error('Delete Error Details (Server Status > 204):', errorText);
                alert(`Error deleting sprint (Status: ${resp.status}). Check console.`);
            }

        } catch (err) {
            // 🛑 CORREÇÃO 3: O bloco 'catch' só deve tratar falhas de rede/execução crítica.
            // A lógica de sucesso (com o reload) deve ser suficiente para evitar que esta mensagem apareça.
            console.error('Critical Error on Delete Execution:', err);
            alert('An unexpected error occurred during deletion. See console for details.');
        }
    });

    // --- Quando o modal abrir ---
    editModalEl.addEventListener('show.bs.modal', () => {
        loadActiveSprint();
    });
});


// --- gannt ---




// ===================================================================================
// GANTT RENDERING
// ===================================================================================
document.addEventListener('DOMContentLoaded', async function () {
    const headerEl = document.getElementById('gantt-header');
    const bodyEl = document.getElementById('gantt-body');

    function dateDiffInDays(a, b) {
        const _MS_PER_DAY = 1000 * 60 * 60 * 24;
        const utc1 = Date.UTC(a.getFullYear(), a.getMonth(), a.getDate());
        const utc2 = Date.UTC(b.getFullYear(), b.getMonth(), b.getDate());
        return Math.floor((utc2 - utc1) / _MS_PER_DAY);
    }

    async function renderGantt() {
        try {
            const res = await fetch('/Sprint/GetActive');
            if (!res.ok) throw new Error("No active sprint found");

            const sprint = await res.json();
            const startDate = new Date(sprint.startDate);
            const endDate = new Date(sprint.endDate);
            const totalDays = dateDiffInDays(startDate, endDate) + 1;

            const visibleDays = Math.min(totalDays, 62);

            const months = [];
            let currentDate = new Date(startDate);

            for (let i = 0; i < visibleDays; i++) {
                const monthName = currentDate.toLocaleString('en-US', { month: 'long' });
                const day = currentDate.getDate();

                if (!months.length || months[months.length - 1].name !== monthName) {
                    months.push({ name: monthName, days: [] });
                }

                months[months.length - 1].days.push(day);
                currentDate.setDate(currentDate.getDate() + 1);
            }

            let monthRow = `<div class="gantt-month-row">`;
            let dayRow = `<div class="gantt-day-row">`;
            const totalDaysInChart = months.reduce((sum, m) => sum + m.days.length, 0);

            months.forEach(month => {
                const widthPercent = (month.days.length / totalDaysInChart) * 100;
                monthRow += `<div class="gantt-cell" style="width:${widthPercent}%; font-weight:bold;">${month.name}</div>`;
                month.days.forEach(day => {
                    dayRow += `<div class="gantt-cell" style="width:${100 / totalDaysInChart}%;">${day}</div>`;
                });
            });

            monthRow += `</div>`;
            dayRow += `</div>`;
            headerEl.innerHTML = monthRow + dayRow;

            bodyEl.innerHTML = '';
            const chartWidth = bodyEl.offsetWidth || 850;
            const dayWidth = chartWidth / totalDaysInChart;

            const bar = document.createElement('div');
            bar.className = 'gantt-bar sprint-bar';
            bar.style.left = `0px`;
            bar.style.width = `${totalDays * dayWidth}px`;
            bar.style.top = '40px';
            bar.textContent = sprint.name || 'Sprint';

            bodyEl.appendChild(bar);

        } catch (err) {
            headerEl.innerHTML = `<div class="text-center text-muted p-3">No active sprint found</div>`;
        }
    }

    renderGantt();
});


// ===================================================================================
// VARIABLES
// ===================================================================================
let sprintChart = null;
let activeSprint = null;
let allFinishedSprints = [];


// ===================================================================================
// INITIALIZATION
// ===================================================================================
document.addEventListener("DOMContentLoaded", () => {
    setupFinishSprintModal();
    loadSprintHistory();
});


// ===================================================================================
// FINISH SPRINT MODAL
// ===================================================================================
function setupFinishSprintModal() {

    const modalEl = document.getElementById("finishedSprintModal");
    const sprintForm = document.getElementById("sprintResultsForm");
    const noSprintSection = document.getElementById("noSprintSection");
    const slider = document.getElementById("progressSlider");
    const label = document.getElementById("progressValue");

    if (!modalEl) return;

    modalEl.addEventListener("shown.bs.modal", async () => {

        const sprint = await fetch("/Sprint/GetActive")
            .then(res => res.ok ? res.json() : null)
            .catch(() => null);

        if (!sprint) {
            noSprintSection.classList.remove("d-none");
            sprintForm.classList.add("d-none");
            return;
        }

        activeSprint = sprint;

        noSprintSection.classList.add("d-none");
        sprintForm.classList.remove("d-none");

        document.getElementById("resultSprintName").innerText = sprint.name;
        document.getElementById("resultSprintGoal").innerText = sprint.goal;

        const start = new Date(sprint.startDate);
        const end = new Date(sprint.endDate);
        const duration = Math.ceil((end - start) / (1000 * 60 * 60 * 24));

        document.getElementById("resultSprintDuration").innerText = `${duration} days`;

        setupSprintChart();
        setTimeout(() => sprintChart?.resize(), 100);
    });


    // ------------------- SETUP DO CHART -------------------
    function setupSprintChart() {
        const ctx = document.getElementById("progressPieChart");
        if (!ctx) return;

        if (sprintChart) sprintChart.destroy();

        const value = parseInt(slider.value);
        label.textContent = value + "%";

        sprintChart = new Chart(ctx, {
            type: "doughnut",
            data: {
                labels: ["Completed", "Pending"],
                datasets: [{
                    data: [value, 100 - value],
                    backgroundColor: ["#1e7e34", "#adb5bd"]
                }]
            },
            options: {
                cutout: "60%",
                responsive: true,
                maintainAspectRatio: false
            }
        });

        slider.addEventListener("input", () => {
            const v = parseInt(slider.value);
            label.textContent = v + "%";
            sprintChart.data.datasets[0].data = [v, 100 - v];
            sprintChart.update();
        });
    }


    // ------------------- SAVE BUTTON HANDLER -------------------
    document.getElementById("saveSprintResultsBtn")?.addEventListener("click", async () => {

        if (!activeSprint) {
            alert("Error: No sprint loaded.");
            return;
        }

        const finishedData = {
            SprintId: activeSprint.id,
            Name: activeSprint.name,
            Goal: activeSprint.goal,
            StartDate: activeSprint.startDate,
            PlannedEndDate: activeSprint.endDate,
            CompletionPercentage: parseInt(slider.value),
            Blockers: document.getElementById("blockersText").value,
            WorkedWell: document.getElementById("workedWellText").value,
            Improvement: document.getElementById("improvementText").value,
            ActualFinishDate: new Date().toISOString()
        };

        try {
            const res = await fetch("/Sprint/Finish", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(finishedData)
            });

            if (!res.ok) {
                alert("Could not finish sprint.");
                return;
            }

            alert("Sprint successfully finished!");

            const bsModal = bootstrap.Modal.getInstance(modalEl);
            bsModal.hide();

            loadSprintHistory();

        } catch (err) {
            console.error(err);
            alert("Communication error.");
        }
    });
}






// ===================================================================================
//  SECTION 2 — SPRINT HISTORY UI
// ===================================================================================
async function loadSprintHistory() {

    const container = document.getElementById("finishedSprintsContainer");
    container.innerHTML = "<p class='text-muted'>Loading sprint history...</p>";

    try {
        const response = await fetch("/Sprint/GetFinished");
        const sprints = await response.json();

        allFinishedSprints = sprints;
        container.innerHTML = "";

        if (sprints.length === 0) {
            container.innerHTML = "<p class='text-muted'>No finished sprints yet.</p>";
            return;
        }

        const latestTwo = sprints.slice(0, 2);

        latestTwo.forEach(renderSprintCard);

        if (sprints.length > 2) {
            document.getElementById("viewAllSprintsWrapper").classList.remove("d-none");
        }

        loadAllSprintsModal(sprints);

    } catch (err) {
        console.error(err);
        container.innerHTML = "<p class='text-danger'>Error loading sprint history.</p>";
    }
}

// ------------------------------
// CREATE CARD FOR EACH FINISHED SPRINT
// ------------------------------
function renderSprintCard(s) {
    const container = document.getElementById("finishedSprintsContainer");

    // 💡 REMOVEMOS p-3 PARA DIMINUIR O PADDING E USAMOS gap-2
    // No HTML, o container deve ter gap-2: <div id="finishedSprintsContainer" class="d-grid gap-2">
    const card = document.createElement("div");
    card.classList.add("card", "shadow-sm"); // Remove p-3 daqui

    // Monta o card
    const cardHtml = `
        <div class="d-flex align-items-center p-2 justify-content-between">
            
            <div class="flex-grow-1 me-2"> 
                <h6 class="mb-0 fw-bold">${s.name}</h6>
                <small class="text-muted">${formatDate(s.startDate)} – ${formatDate(s.plannedEndDate)}</small>
            </div>

            <div class="d-flex align-items-center gap-2"> 
                
                <div style="width: 50px; height: 50px;"> 
                    <canvas id="chart-${s.id}"></canvas>
                </div>

                <div class="text-end me-2"> 
                    <h5 class="mb-0 fw-bold text-success">${s.completionPercentage}%</h5> 
                    <small class="text-muted">productivity</small>
                </div>

                <button class="btn btn-sm btn-success rounded-circle view-details-btn"
                        data-sprint='${JSON.stringify(s)}'
                        style="width: 32px; height: 32px; display: flex; align-items: center; justify-content: center;">
                    <i class="fas fa-plus"></i>
                </button>

            </div>
        </div>
    `;

    // 💡 INJETAMOS O HTML DENTRO DO CARD, NÃO NO CONTAINER.
    // Isso é mais seguro e garante que os estilos customizados sejam aplicados.
    card.innerHTML = cardHtml;
    container.appendChild(card);

    renderHistoryChart(`chart-${s.id}`, s.completionPercentage);
}
// ------------------------------
// MINI PIE CHART FOR HISTORY CARDS
// ------------------------------
function renderHistoryChart(canvasId, pct) {
    const ctx = document.getElementById(canvasId);

    new Chart(ctx, {
        type: "doughnut",
        data: {
            datasets: [{
                data: [pct, 100 - pct],
                backgroundColor: ["#28a745", "#ffc107"],
                borderWidth: 0
            }]
        },
        options: {
            cutout: "50%",
            responsive: true
        }
    });
}

// ===================================================================================
//  SECTION 3 — VIEW ALL SPRINTS
// ===================================================================================
function loadAllSprintsModal(sprints) {
    const list = document.getElementById("allSprintsList");
    list.innerHTML = "";

    sprints.forEach(s => {
        list.insertAdjacentHTML("beforeend", `
            <div class="border rounded p-3 d-flex justify-content-between align-items-center">

                <div>
                    <h6 class="fw-bold mb-1 text-truncate" style="max-width: 160px;">
                        ${s.name}
                    </h6>
                    <small class="text-muted">${formatDate(s.startDate)} – ${formatDate(s.plannedEndDate)}</small>
                </div>

                <div class="d-flex align-items-center gap-3">

                    <span class="fw-bold text-success">${s.completionPercentage}%</span>

                    <!-- 🔥 Botão verde com ícone + -->
                    <button class="btn btn-success btn-sm rounded-circle view-details-btn"
                            data-sprint='${JSON.stringify(s)}'
                            style="width: 32px; height: 32px; display:flex; align-items:center; justify-content:center;">
                        <i class="fas fa-plus"></i>
                    </button>

                </div>

            </div>
        `);
    });
}

// ===================================================================================
//  SECTION 4 — SPRINT DETAILS
// ===================================================================================
document.addEventListener("click", (event) => {
    const btn = event.target.closest(".view-details-btn");
    if (!btn) return;

    const sprint = JSON.parse(btn.dataset.sprint);
    openSprintDetailsModal(sprint);
});

function openSprintDetailsModal(s) {
    document.getElementById("detailTitle").textContent = s.name;
    document.getElementById("detailDates").textContent =
        `${formatDate(s.startDate)} → ${formatDate(s.actualFinishDate)}`;

    document.getElementById("detailGoal").textContent = s.goal || "—";
    document.getElementById("detailWorked").textContent = s.workedWell || "—";
    document.getElementById("detailImprovement").textContent = s.improvement || "—";
    document.getElementById("detailBlockers").textContent = s.blockers || "—";

    new bootstrap.Modal(document.getElementById("sprintDetailsModal")).show();
}

// ===================================================================================
//  UTILS
// ===================================================================================
function formatDate(dateStr) {
    return new Date(dateStr).toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric"
    });
}









// ----------------------------------------------------------------------
// FUNÇÃO PRINCIPAL DE INICIALIZAÇÃO E LIMPEZA (ÚNICA CHAMADA)
// ----------------------------------------------------------------------
document.addEventListener('DOMContentLoaded', async () => {
    const container = document.getElementById("impactNotificationsContainer");

    // 1. VERIFICAÇÃO CRÍTICA DO CONTAINER
    if (!container) {
        console.error("ERRO CRÍTICO: Container #impactNotificationsContainer não encontrado.");
        return;
    }

    // Zera o container ANTES de qualquer carregamento.
    container.innerHTML = "";

    // 2. Chama as funções sequencialmente. O await garante que a execução termine antes de seguir.
    // Usamos await para que o erro de uma não interrompa a injeção da outra.

    // 🚨 1º Chamada: Licença Individual (que não estava aparecendo)
    await loadAbsenceImpact();

    // 🚨 2º Chamada: Feriado (que estava aparecendo)
    await loadSprintHolidayNotifications();

    // 3. Se o container estiver vazio no final, adiciona mensagem de fallback.
    if (container.innerHTML.trim() === "") {
        container.innerHTML = '<div class="text-muted small">Nenhuma notificação de impacto para esta sprint.</div>';
    }
});
// (As funções loadAbsenceImpact e loadSprintHolidayNotifications devem ser definidas abaixo)

// ----------------------------------------------------------------------
// FUNÇÃO 1: Notificação de Feriado (SÓ ADICIONA CONTEÚDO)
// ----------------------------------------------------------------------
async function loadSprintHolidayNotifications() {
    const container = document.getElementById('impactNotificationsContainer');
    if (!container) return; // Proteção

    try {
        const res = await fetch('/Sprint/GetHolidayNotifications');
        if (!res.ok) return;

        const notifications = await res.json();
        if (!notifications || notifications.length === 0) return;

        notifications.forEach(n => {
            const percent = n.affectedPercent ?? 0;
            const alertClass = percent >= 50 ? 'alert-danger' : 'alert-warning';

            const html = `
                <div class="alert ${alertClass} d-flex align-items-center mb-2" role="alert">
                    ${n.message}
                </div>
            `;
            // 🚨 APENAS ADICIONAMOS (appendChild)
            container.insertAdjacentHTML('beforeend', html);
        });

    } catch (err) {
        console.error('Falha ao carregar notificações de feriado:', err);
    }
}


// ----------------------------------------------------------------------
// FUNÇÃO 2: Notificação de Ausência Individual (SÓ ADICIONA CONTEÚDO)
// ----------------------------------------------------------------------
async function loadAbsenceImpact() {
    const container = document.getElementById("impactNotificationsContainer");
    if (!container) return; // Proteção

    try {
        const response = await fetch("/Sprint/GetAbsenceImpact");
        if (!response.ok) return;

        const data = await response.json();
        if (data.length === 0) return;

        data.forEach(n => {
            const icon = n.ImpactPercent >= 10 ? '🔥' : '⚡';
            const alertClass = n.ImpactPercent >= 10 ? 'alert-danger' : 'alert-info';

            const html = `
                <div class="alert ${alertClass} small mb-0 d-flex align-items-start p-2" role="alert" style="border-left: 4px solid;">
                    <span class="me-2 fs-5" style="font-size: 24px;">${icon}</span>
                    <div class="flex-grow-1">${n.message}</div>
                </div>
            `;
            // APENAS ADICIONAMOS
            container.insertAdjacentHTML('beforeend', html);
        });

    } catch (err) {
        console.error("Erro fatal na função loadAbsenceImpact:", err);
    }
}



// ======================================================================
//  SPRINT HISTORY — COLLABORATOR VIEW
// ======================================================================

// ------------------------------
// 1. LOAD HISTORY
// ------------------------------
async function loadSprintHistory() {

    const container = document.getElementById("finishedSprintsContainer");

    if (!container) {
        console.warn("finishedSprintsContainer not found (collaborator view).");
        return;
    }

    container.innerHTML = `
        <div class="text-center text-muted py-3">Loading history...</div>
    `;

    try {
        const response = await fetch("/Sprint/GetFinished");
        const sprints = await response.json();

        container.innerHTML = "";

        if (!sprints || sprints.length === 0) {
            container.innerHTML = `
                <div class="text-center text-muted p-3">
                    No finished sprints yet.
                </div>`;
            return;
        }

        // render only the last 2 on home
        const latestTwo = sprints.slice(0, 2);
        latestTwo.forEach(renderSprintCard);

        // Show "view all" button if more exist
        if (sprints.length > 2) {
            const wrapper = document.getElementById("viewAllSprintsWrapper");
            if (wrapper) wrapper.classList.remove("d-none");
        }

        loadAllSprintsModal(sprints);

    } catch (err) {
        console.error("Error loading sprint history:", err);
        container.innerHTML = `
            <div class="text-danger text-center p-3">Error loading history.</div>
        `;
    }
}



// ------------------------------
// 2. CARD COMPONENT
// ------------------------------
function renderSprintCard(s) {
    const container = document.getElementById("finishedSprintsContainer");
    if (!container) return;

    const card = document.createElement("div");
    card.classList.add("card", "shadow-sm", "mb-2");

    card.innerHTML = `
        <div class="d-flex align-items-center p-2 justify-content-between">

            <div class="flex-grow-1 me-2">
                <h6 class="mb-0 fw-bold">${s.name}</h6>
                <small class="text-muted">${formatDate(s.startDate)} – ${formatDate(s.plannedEndDate)}</small>
            </div>

            <div class="d-flex align-items-center gap-2">

                <div style="width: 48px; height: 48px;">
                    <canvas id="chart-${s.id}"></canvas>
                </div>

                <div class="text-end me-2">
                    <h5 class="mb-0 fw-bold text-success">${s.completionPercentage}%</h5>
                    <small class="text-muted">productivity</small>
                </div>

                <button class="btn btn-sm btn-success rounded-circle view-details-btn"
                        data-sprint='${JSON.stringify(s)}'
                        style="width: 32px; height: 32px; display:flex; align-items:center; justify-content:center;">
                    <i class="fas fa-plus"></i>
                </button>

            </div>
        </div>
    `;

    container.appendChild(card);

    renderHistoryChart(`chart-${s.id}`, s.completionPercentage);
}



// ------------------------------
// 3. MINI PIE CHART
// ------------------------------
function renderHistoryChart(canvasId, pct) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    new Chart(ctx, {
        type: "doughnut",
        data: {
            datasets: [{
                data: [pct, 100 - pct],
                backgroundColor: ["#28a745", "#ffc107"],
                borderWidth: 0
            }]
        },
        options: {
            cutout: "55%",
            responsive: true,
            plugins: { legend: { display: false } }
        }
    });
}



// ======================================================================
// 4. VIEW ALL SPRINTS — MODAL LIST
// ======================================================================
function loadAllSprintsModal(sprints) {
    const list = document.getElementById("allSprintsList");
    if (!list) return;

    list.innerHTML = "";

    sprints.forEach(s => {
        list.insertAdjacentHTML("beforeend", `
            <div class="border rounded p-3 d-flex justify-content-between align-items-center mb-2">

                <div>
                    <h6 class="fw-bold mb-1">${s.name}</h6>
                    <small class="text-muted">${formatDate(s.startDate)} – ${formatDate(s.plannedEndDate)}</small>
                </div>

                <div class="d-flex align-items-center gap-3">

                    <span class="fw-bold text-success">${s.completionPercentage}%</span>

                    <button class="btn btn-success btn-sm rounded-circle view-details-btn"
                            data-sprint='${JSON.stringify(s)}'
                            style="width: 32px; height: 32px; display:flex; align-items:center; justify-content:center;">
                        <i class="fas fa-plus"></i>
                    </button>

                </div>
            </div>
        `);
    });
}



// ======================================================================
// 5. OPEN DETAILS MODAL
// ======================================================================
document.addEventListener("click", (event) => {
    const btn = event.target.closest(".view-details-btn");
    if (!btn) return;

    const sprint = JSON.parse(btn.dataset.sprint);
    openSprintDetailsModal(sprint);
});


function openSprintDetailsModal(s) {

    document.getElementById("detailTitle").textContent = s.name;

    document.getElementById("detailDates").textContent =
        `${formatDate(s.startDate)} → ${formatDate(s.actualFinishDate)}`;

    document.getElementById("detailGoal").textContent = s.goal || "—";
    document.getElementById("detailWorked").textContent = s.workedWell || "—";
    document.getElementById("detailImprovement").textContent = s.improvement || "—";
    document.getElementById("detailBlockers").textContent = s.blockers || "—";

    new bootstrap.Modal(document.getElementById("sprintDetailsModal")).show();
}



// ======================================================================
// 6. UTILS
// ======================================================================
function formatDate(dateStr) {
    return new Date(dateStr).toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric"
    });
}



// ======================================================================
// 7. AUTO LOAD WHEN PAGE OPENS
// ======================================================================
document.addEventListener("DOMContentLoaded", () => {
    loadSprintHistory();
});









