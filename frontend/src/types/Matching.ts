export interface TecnicoCompativel {
    id: number
    nomeExibicao: string
    descricaoProfissional: string
    cidadeAtendimento: string
    estadoAtendimento: string
    disponivel: boolean
    especialidadeId: number
    especialidade: string
    nivelExperiencia: string
    anosExperiencia: number
    mesmaCidade: boolean
    pontuacao: number
}

export interface SelecaoTecnico {
    id: number
    solicitacaoId: number
    tecnicoId: number
    tecnico?: string
    especialidadeId: number
    especialidade: string
    status: string
    dataSelecao: string
    dataResposta?: string
}

export interface SelecionarTecnicoDados {
    solicitacaoId: number
    tecnicoId: number
    especialidadeId: number
}

export interface HistoricoSelecao {
    id: number
    selecaoTecnicoId: number
    status: string
    observacao: string
    dataRegistro: string
}