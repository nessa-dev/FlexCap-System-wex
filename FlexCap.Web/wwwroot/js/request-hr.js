console.log("✅ request-hr.js carregado com sucesso!");

document.addEventListener("DOMContentLoaded", () => {
    console.log("🧠 Testando abertura do modal HR...");

    const detailModal = document.getElementById("requestDetailModal");
    const urlRequestDetails = "/HRApproval/GetRequestDetails";

    if (detailModal) {
        detailModal.addEventListener("show.bs.modal", async (event) => {
            const button = event.relatedTarget;
            const requestId = button?.getAttribute("data-request-id");

            if (!requestId) {
                console.error("❌ Nenhum requestId encontrado no botão.");
                return;
            }

            console.log(`📦 Modal aberto — ID capturado: ${requestId}`);
            const loading = document.getElementById("loadingDetails");
            const content = document.getElementById("requestDetailsContent");

            loading.style.display = "block";
            content.style.display = "none";

            try {
                const response = await fetch(`${urlRequestDetails}?requestId=${requestId}`);
                const data = await response.json();

                console.log("📄 Dados recebidos:", data);

                if (data.error) throw new Error(data.error);

                // Preenche os campos
                document.getElementById("detailCollaboratorName").textContent = data.collaboratorName || "-";
                document.getElementById("detailCollaboratorPosition").textContent = data.collaboratorPosition || "-";
                document.getElementById("detailCollaboratorDepartment").textContent = data.collaboratorDepartment || "-";
                document.getElementById("detailCurrentStatus").textContent = data.currentStatus || "-";
                document.getElementById("detailStartDate").textContent = data.startDate || "-";
                document.getElementById("detailEndDate").textContent = data.endDate || "-";
                document.getElementById("detailTypeName").textContent = data.typeName || "-";
                document.getElementById("detailSubject").textContent = data.subject || "(Sem assunto)";
                document.getElementById("detailDescription").textContent = data.description || "(Sem descrição)";

                const attachmentLink = document.getElementById("detailAttachmentLink");
                const attachmentName = document.getElementById("detailAttachmentName");
                const noAttachmentText = document.getElementById("noAttachmentText");

                if (data.attachmentPath) {
                    const attachmentLink = document.getElementById('detailAttachmentLink');
                    const fileName = data.attachmentPath.split("/").pop();

                    document.getElementById("detailAttachmentName").textContent = fileName;

                    // 🔗 Define o link correto para baixar
                    attachmentLink.href = `/HRApproval/DownloadAttachment?requestId=${data.requestId}`;
                    attachmentLink.style.display = "inline-block";
                    noAttachmentText.style.display = "none";
                } else {
                    attachmentLink.style.display = "none";
                    noAttachmentText.style.display = "block";
                }


                loading.style.display = "none";
                content.style.display = "block";
            } catch (err) {
                console.error("❌ Erro ao carregar detalhes:", err);
                loading.innerHTML = `<p class="text-danger">Erro ao carregar detalhes da solicitação.</p>`;
            }
        });
    }

    // ------------------------------
    // MODAL DE REJEIÇÃO
    // ------------------------------
    function setupModal(modalId, hiddenInputId) {
        const modal = document.getElementById(modalId);
        if (!modal) return;

        modal.addEventListener("show.bs.modal", (event) => {
            const button = event.relatedTarget;
            const requestId = button?.getAttribute("data-request-id");
            console.log(`🆔 Abertura de modal: ${modalId}, RequestId = ${requestId}`);

            const hiddenInput = modal.querySelector(`#${hiddenInputId}`);
            if (hiddenInput) hiddenInput.value = requestId;
        });
    }

    setupModal("rejectHRModal", "modal-reject-hr-request-id");
    setupModal("returnManagerModal", "modal-return-manager-request-id");
});
