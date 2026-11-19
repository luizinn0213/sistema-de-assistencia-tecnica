export interface TecnicoRanking {
    id: number
    nomeExibicao: string
    descricaoProfissional: string
    cidadeAtendimento: string
    estadoAtendimento: string
    especialidadeId: number
    especialidade: string
    nivelExperiencia: number | string
    anosExperiencia: number
    mesmaCidade: boolean
    pontuacao: number
    destaques: string[]
}

export interface SelecaoTecnico {
    id: number
    solicitacaoId: number
    solicitacaoNumero?: string
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
