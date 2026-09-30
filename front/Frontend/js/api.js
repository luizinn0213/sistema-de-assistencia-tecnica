// Configuração da API
const API_BASE_URL = 'https://localhost:5001/api';

class ApiService {
    static async request(endpoint, method = 'GET', body = null) {
        try {
            const options = {
                method,
                headers: { 'Content-Type': 'application/json' },
            };
            if (body) options.body = JSON.stringify(body);

            const response = await fetch(`${API_BASE_URL}${endpoint}`, options);
            if (!response.ok) {
                const errorData = await response.json().catch(() => null);
                throw new Error(errorData?.message || `Erro ${response.status}`);
            }
            return await response.json();
        } catch (error) {
            console.error('Erro na requisição:', error);
            throw error;
        }
    }

    static async finalizarServico(data) {
        return this.request('/solicitacoes/finalizar', 'POST', data);
    }

    static async obterHistorico(solicitacaoId) {
        return this.request(`/solicitacoes/${solicitacaoId}/historico`);
    }
}