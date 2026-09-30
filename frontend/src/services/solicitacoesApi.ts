import type {
    CriarSolicitacaoDados,
    Solicitacao
} from '../types/Solicitacao'

const API_URL =
    'http://localhost:5141/api/solicitacoes'

async function obterErro(
    resposta: Response
): Promise<string> {
    const texto = await resposta.text()

    if (!texto) {
        return 'Não foi possível concluir a operação.'
    }

    try {
        const dados = JSON.parse(texto)

        if (dados.errors) {
            const mensagens = Object.values(
                dados.errors
            ).flat()

            if (mensagens.length > 0) {
                return String(mensagens[0])
            }
        }

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

export async function abrirSolicitacao(
    clienteId: number,
    dados: CriarSolicitacaoDados
): Promise<Solicitacao> {
    const resposta = await fetch(
        `${API_URL}/cliente/${clienteId}`,
        {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dados)
        }
    )

    if (!resposta.ok) {
        throw new Error(await obterErro(resposta))
    }

    return resposta.json()
}

export async function listarSolicitacoes(
    clienteId: number
): Promise<Solicitacao[]> {
    const resposta = await fetch(
        `${API_URL}/cliente/${clienteId}`
    )

    if (!resposta.ok) {
        throw new Error(await obterErro(resposta))
    }

    return resposta.json()
}
