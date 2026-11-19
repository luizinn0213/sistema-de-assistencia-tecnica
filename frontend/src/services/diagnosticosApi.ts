import type {
    AtualizarDiagnosticoDados,
    CriarDiagnosticoDados,
    Diagnostico
} from '../types/DiagnosticoOrcamento'

const API_URL =
    'http://localhost:5141/api/diagnosticos'

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

async function processarResposta<T>(
    resposta: Response
): Promise<T> {
    if (!resposta.ok) {
        throw new Error(
            await obterMensagemErro(resposta)
        )
    }

    return resposta.json()
}

export async function criarDiagnostico(
    dados: CriarDiagnosticoDados
): Promise<Diagnostico> {
    const resposta = await fetch(API_URL, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(dados)
    })

    return processarResposta<Diagnostico>(
        resposta
    )
}

export async function obterDiagnostico(
    id: number
): Promise<Diagnostico> {
    const resposta = await fetch(
        `${API_URL}/${id}`
    )

    return processarResposta<Diagnostico>(
        resposta
    )
}

export async function obterDiagnosticoPorSolicitacao(
    solicitacaoId: number
): Promise<Diagnostico> {
    const resposta = await fetch(
        `${API_URL}/solicitacao/${solicitacaoId}`
    )

    return processarResposta<Diagnostico>(
        resposta
    )
}

export async function atualizarDiagnostico(
    id: number,
    dados: AtualizarDiagnosticoDados
): Promise<Diagnostico> {
    const resposta = await fetch(
        `${API_URL}/${id}`,
        {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dados)
        }
    )

    return processarResposta<Diagnostico>(
        resposta
    )
}

export async function concluirDiagnostico(
    id: number
): Promise<Diagnostico> {
    const resposta = await fetch(
        `${API_URL}/${id}/concluir`,
        {
            method: 'POST'
        }
    )

    return processarResposta<Diagnostico>(
        resposta
    )
}