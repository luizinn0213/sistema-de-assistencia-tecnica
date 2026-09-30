import ApiService from './apiService.js';

// Aguardar o DOM carregar
document.addEventListener('DOMContentLoaded', () => App.init());

class App {
    static init() {
        this.configurarFormFinalizar();
        this.configurarBtnHistorico();
    }

    /* ---------- FORMULÁRIO FINALIZAR ---------- */
    static configurarFormFinalizar() {
        const form = document.getElementById('form-finalizar');
        
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const data = {
                solicitacaoId: parseInt(document.getElementById('fin-solicitacao-id').value),
                tecnicoId: parseInt(document.getElementById('fin-tecnico-id').value),
                solucaoAplicada: document.getElementById('fin-solucao').value,
                observacaoFinal: document.getElementById('fin-observacao').value || null
            };

            try {
                // Chama a API
                const resultado = await ApiService.finalizarServico(data);
                
                // Mostra sucesso
                this.showNotification('Serviço finalizado com sucesso!', 'success');
                
                // Limpa o formulário
                form.reset();
                
                console.log('Resultado:', resultado);
            } catch (error) {
                this.showNotification(`Erro: ${error.message}`, 'error');
            }
        });
    }

    /* ---------- HISTÓRICO ---------- */
    static configurarBtnHistorico() {
        document.getElementById('btn-view-historico').addEventListener('click', () => {
            const id = document.getElementById('view-solicitacao-id').value;
            
            if (!id) {
                this.showNotification('Digite um ID de solicitação', 'error');
                return;
            }
            
            this.visualizarHistorico(parseInt(id));
        });
    }

    static async visualizarHistorico(solicitacaoId) {
        try {
            // Mostra loading
            document.getElementById('historico-list').innerHTML = 
                '<div class="loading">Carregando</div>';
            
            // Chama a API
            const historico = await ApiService.obterHistorico(solicitacaoId);
            
            // Limpa a lista
            document.getElementById('historico-list').innerHTML = '';
            
            if (!historico.length) {
                document.getElementById('historico-list').innerHTML = 
                    '<p style="color:var(--gray-500);padding:16px;">Nenhum histórico encontrado.</p>';
                return;
            }
            
            // Renderiza cada item
            historico.forEach(h => {
                const data = new Date(h.dataRegistro).toLocaleString('pt-BR');
                const html = `
                    <div class="timeline-item">
                        <div class="tipo">${h.tipo}</div>
                        <div class="descricao">${h.descricao}</div>
                        <div class="data">Técnico: ${h.tecnicoNome} · ${data}</div>
                    </div>
                `;
                document.getElementById('historico-list').insertAdjacentHTML('beforeend', html);
            });
        } catch (error) {
            this.showNotification(`Erro ao carregar histórico: ${error.message}`, 'error');
        }
    }

    /* ---------- NOTIFICAÇÃO ---------- */
    static showNotification(message, type = 'success') {
        const toast = document.getElementById('notification');
        const msg = document.getElementById('notification-message');
        
        msg.textContent = message;
        toast.className = `toast ${type}`;
        
        setTimeout(() => toast.classList.add('hidden'), 3500);
    }
}