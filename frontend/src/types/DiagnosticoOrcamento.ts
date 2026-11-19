export type StatusDiagnostico =
    | 'EmElaboracao'
    | 'Concluido'

export interface Diagnostico {
    id: number
    solicitacaoId: number
    solicitacaoNumero?: string
    tecnicoId: number
    tecnico?: string
    descricaoDefeito: string
    solucaoRecomendada: string
    status: StatusDiagnostico
    dataCriacao: string
    dataConclusao?: string
}

export interface CriarDiagnosticoDados {
    solicitacaoId: number
    descricaoDefeito: string
}

export interface AtualizarDiagnosticoDados {
    descricaoDefeito: string
    solucaoRecomendada: string
}

export type TipoItemOrcamento =
    | 'Peca'
    | 'Servico'

export interface ItemOrcamento {
    id: number
    descricao: string
    tipo: TipoItemOrcamento
    quantidade: number
    valorUnitario: number
    valorTotal: number
}

export interface HistoricoOrcamento {
    id: number
    status: string
    observacao: string
    dataRegistro: string
}

export interface Orcamento {
    id: number
    diagnosticoId: number
    solicitacaoId?: number
    solicitacaoNumero?: string
    status: string
    dataCriacao: string
    dataEnvio?: string
    dataResposta?: string
    valorTotal: number
    itens: ItemOrcamento[]
    historico: HistoricoOrcamento[]
}

export interface ItemOrcamentoDados {
    descricao: string
    tipo: 1 | 2
    quantidade: number
    valorUnitario: number
}

export interface RespostaOrcamentoDados {
    observacao: string
}