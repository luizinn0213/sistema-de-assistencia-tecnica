export interface Solicitacao {
    id: number
    numero: string
    descricaoProblema: string
    status: string
    dataCriacao: string
    clienteId: number
    equipamento: {
        id: number
        tipo: string
        marca: string
        modelo: string
    }
    especialidade: {
        id: number
        nome: string
    }
}

// Número, data e status são definidos pelo backend.
export interface CriarSolicitacaoDados {
    equipamentoId: number
    especialidadeId: number
    descricaoProblema: string
}
