import API_CONFIG from './config.js';

class ApiService {
    /**
     * Método genérico para fazer requisições
     */
    static async request(endpoint, method = 'GET', body = null) {
        try {
            const options = {
                method,
                headers: API_CONFIG.HEADERS,
                credentials: 'include' // Para cookies/autenticação
            };

            if (body) {
                options.body = JSON.stringify(body);
            }

            const response = await fetch(`${API_CONFIG.BASE_URL}${endpoint}`, options);

            // Verifica se a resposta foi bem-sucedida
            if (!response.ok) {
                const errorData = await response.json().catch(() => null);
                throw new Error(errorData?.message || `Erro ${response.status}: ${response.statusText}`);
            }

            // Se não houver conteúdo (204 No Content)
            if (response.status === 204) {
                return null;
            }

            return await response.json();
        } catch (error) {
            console.error('Erro na requisição:', error);
            throw error;
        }
    }

    // ============ MÉTODOS DA API ============

    /**
     * Finalizar serviço
     */
    static async finalizarServico(data) {
        return this.request('/solicitacoes/finalizar', 'POST', data);
    }

    /**
     * Obter histórico de uma solicitação
     */
    static async obterHistorico(solicitacaoId) {
        return this.request(`/solicitacoes/${solicitacaoId}/historico`);
    }

    /**
     * Registrar histórico
     */
    static async registrarHistorico(data) {
        return this.request('/solicitacoes/historico', 'POST', data);
    }

    /**
     * Obter todas as solicitações
     */
    static async obterSolicitacoes() {
        return this.request('/solicitacoes');
    }
}

export default ApiService;