class UIComponents {
    static showNotification(message, type = 'success') {
        const toast = document.getElementById('notification');
        const msg = document.getElementById('notification-message');
        msg.textContent = message;
        toast.className = `toast ${type}`;
        setTimeout(() => toast.classList.add('hidden'), 3500);
    }

    static showLoading(elementId) {
        document.getElementById(elementId).innerHTML = '<div class="loading">Carregando</div>';
    }

    static clearElement(elementId) {
        document.getElementById(elementId).innerHTML = '';
    }

    static renderHistoricoItem(h) {
        const data = new Date(h.dataRegistro).toLocaleString('pt-BR');
        return `
            <div class="timeline-item">
                <div class="tipo">${h.tipo}</div>
                <div class="descricao">${h.descricao}</div>
                <div class="data">Técnico: ${h.tecnicoNome} · ${data}</div>
            </div>
        `;
    }
}