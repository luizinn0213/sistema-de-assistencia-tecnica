import type {
    Avaliacao,
    CriarAvaliacaoDados,
    ExecucaoServico
} from '../types/ExecucaoAvaliacao'

const EXECUCOES_URL = 'http://localhost:5141/api/execucoes'
const AVALIACOES_URL = 'http://localhost:5141/api/avaliacoes'

async function mensagemErro(resposta: Response): Promise<string> {
    const texto = await resposta.text()

    if (!texto) {
        return 'Não foi possível concluir a operação.'
    }

    try {
        const dados = JSON.parse(texto)
        return dados.erro || dados.message || dados.title || texto
    } catch {
        return texto
    }
}

async function processar<T>(resposta: Response): Promise<T> {
    if (!resposta.ok) {
        throw new Error(await mensagemErro(resposta))
    }

    return resposta.json()
}

export async function criarExecucao(
    orcamentoId: number
): Promise<ExecucaoServico> {
    return processar(await fetch(EXECUCOES_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ orcamentoId })
    }))
}

export async function obterExecucaoPorOrcamento(
    orcamentoId: number
): Promise<ExecucaoServico> {
    return processar(await fetch(
        `${EXECUCOES_URL}/orcamento/${orcamentoId}`
    ))
}

export async function iniciarExecucao(
    id: number,
    observacao: string
): Promise<ExecucaoServico> {
    return processar(await fetch(`${EXECUCOES_URL}/${id}/iniciar`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ observacao })
    }))
}

export async function concluirExecucao(
    id: number,
    observacaoFinal: string
): Promise<ExecucaoServico> {
    return processar(await fetch(`${EXECUCOES_URL}/${id}/concluir`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ observacaoFinal })
    }))
}

export async function criarAvaliacao(
    dados: CriarAvaliacaoDados
): Promise<Avaliacao> {
    return processar(await fetch(AVALIACOES_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(dados)
    }))
}

export async function obterAvaliacaoPorExecucao(
    execucaoServicoId: number
): Promise<Avaliacao> {
    return processar(await fetch(
        `${AVALIACOES_URL}/execucao/${execucaoServicoId}`
    ))
}
