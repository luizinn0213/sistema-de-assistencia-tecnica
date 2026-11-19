export type StatusExecucaoServico =
    | 'AguardandoInicio'
    | 'EmAndamento'
    | 'Concluida'

export interface HistoricoExecucaoServico {
    id: number
    status: StatusExecucaoServico
    observacao: string
    dataRegistro: string
}

export interface ExecucaoServico {
    id: number
    orcamentoId: number
    solicitacaoId?: number
    solicitacaoNumero?: string
    tecnicoId?: number
    tecnico?: string
    status: StatusExecucaoServico
    dataCriacao: string
    dataInicio?: string
    dataConclusao?: string
    observacaoInicio?: string
    observacaoFinal?: string
    historico: HistoricoExecucaoServico[]
}

export interface Avaliacao {
    id: number
    execucaoServicoId: number
    clienteId: number
    cliente?: string
    nota: number
    comentario?: string
    dataAvaliacao: string
}

export interface CriarAvaliacaoDados {
    execucaoServicoId: number
    clienteId: number
    nota: number
    comentario?: string
}
