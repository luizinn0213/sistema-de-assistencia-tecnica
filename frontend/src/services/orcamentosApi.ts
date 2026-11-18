import type {
    ItemOrcamentoDados,
    Orcamento,
    RespostaOrcamentoDados
} from '../types/DiagnosticoOrcamento'

const API_URL =
    'http://localhost:5141/api/orcamentos'

async function obterMensagemErro(
    resposta: Response
): Promise<string> {
    const texto = await resposta.text()

    if (!texto) {
        return 'Não foi possível concluir a operação.'
    }

    try {
        const dados = JSON.parse(texto)

        return (
            dados.erro ||
            dados.message ||
            dados.title ||
            texto
        )
    } catch {
        return texto
    }
}

async function processarResposta(
    resposta: Response
): Promise<Orcamento> {
    if (!resposta.ok) {
        throw new Error(
            await obterMensagemErro(resposta)
        )
    }

    return resposta.json()
}

export async function criarOrcamento(
    diagnosticoId: number
): Promise<Orcamento> {
    const resposta = await fetch(API_URL, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            diagnosticoId
        })
    })

    return processarResposta(resposta)
}

export async function obterOrcamento(
    id: number
): Promise<Orcamento> {
    const resposta = await fetch(
        `${API_URL}/${id}`
    )

    return processarResposta(resposta)
}

export async function obterOrcamentoPorDiagnostico(
    diagnosticoId: number
): Promise<Orcamento> {
    const resposta = await fetch(
        `${API_URL}/diagnostico/${diagnosticoId}`
    )

    return processarResposta(resposta)
}

export async function adicionarItemOrcamento(
    orcamentoId: number,
    dados: ItemOrcamentoDados
): Promise<Orcamento> {
    const resposta = await fetch(
        `${API_URL}/${orcamentoId}/itens`,
        {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dados)
        }
    )

    return processarResposta(resposta)
}

export async function alterarItemOrcamento(
    orcamentoId: number,
    itemId: number,
    dados: ItemOrcamentoDados
): Promise<Orcamento> {
    const resposta = await fetch(
        `${API_URL}/${orcamentoId}/itens/${itemId}`,
        {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dados)
        }
    )

    return processarResposta(resposta)
}

export async function removerItemOrcamento(
    orcamentoId: number,
    itemId: number
): Promise<Orcamento> {
    const resposta = await fetch(
        `${API_URL}/${orcamentoId}/itens/${itemId}`,
        {
            method: 'DELETE'
        }
    )

    return processarResposta(resposta)
}

export async function enviarOrcamento(
    id: number
): Promise<Orcamento> {
    const resposta = await fetch(
        `${API_URL}/${id}/enviar`,
        {
            method: 'POST'
        }
    )

    return processarResposta(resposta)
}

async function responderOrcamento(
    id: number,
    operacao: 'aprovar' | 'rejeitar' | 'cancelar',
    dados: RespostaOrcamentoDados
): Promise<Orcamento> {
    const resposta = await fetch(
        `${API_URL}/${id}/${operacao}`,
        {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dados)
        }
    )

    return processarResposta(resposta)
}

export function aprovarOrcamento(
    id: number,
    dados: RespostaOrcamentoDados
): Promise<Orcamento> {
    return responderOrcamento(
        id,
        'aprovar',
        dados
    )
}

export function rejeitarOrcamento(
    id: number,
    dados: RespostaOrcamentoDados
): Promise<Orcamento> {
    return responderOrcamento(
        id,
        'rejeitar',
        dados
    )
}

export function cancelarOrcamento(
    id: number,
    dados: RespostaOrcamentoDados
): Promise<Orcamento> {
    return responderOrcamento(
        id,
        'cancelar',
        dados
    )
}