import type {
    HistoricoSelecao,
    SelecaoTecnico,
    SelecionarTecnicoDados,
    TecnicoCompativel
} from '../types/Matching'

const API_URL = 'http://localhost:5141/api/matching'

async function obterMensagemErro(
    resposta: Response
): Promise<string> {
    const texto = await resposta.text()

    if (!texto) {
        return 'Não foi possível concluir a operação.'
    }

    try {
        const dados = JSON.parse(texto)

        if (typeof dados === 'string') {
            return dados
        }

        return (
            dados.message ||
            dados.title ||
            texto
        )
    } catch {
        return texto
    }
}

async function validarResposta(
    resposta: Response
): Promise<void> {
    if (!resposta.ok) {
        throw new Error(
            await obterMensagemErro(resposta)
        )
    }
}

export async function buscarTecnicosCompativeis(
    especialidadeId: number,
    cidade: string
): Promise<TecnicoCompativel[]> {
    const parametros = new URLSearchParams({
        especialidadeId: especialidadeId.toString()
    })

    if (cidade.trim()) {
        parametros.append('cidade', cidade.trim())
    }

    const resposta = await fetch(
        `${API_URL}/tecnicos?${parametros.toString()}`
    )

    await validarResposta(resposta)
    return resposta.json()
}

export async function selecionarTecnico(
    dados: SelecionarTecnicoDados
): Promise<SelecaoTecnico> {
    const resposta = await fetch(
        `${API_URL}/selecionar`,
        {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dados)
        }
    )

    await validarResposta(resposta)
    return resposta.json()
}

export async function buscarSelecaoDaSolicitacao(
    solicitacaoId: number
): Promise<SelecaoTecnico> {
    const resposta = await fetch(
        `${API_URL}/solicitacoes/${solicitacaoId}`
    )

    await validarResposta(resposta)
    return resposta.json()
}

export async function listarSelecoesPendentes(
    tecnicoId: number
): Promise<SelecaoTecnico[]> {
    const resposta = await fetch(
        `${API_URL}/tecnicos/${tecnicoId}/pendentes`
    )

    await validarResposta(resposta)
    return resposta.json()
}

export async function aceitarSelecao(
    selecaoId: number
): Promise<void> {
    const resposta = await fetch(
        `${API_URL}/selecoes/${selecaoId}/aceitar`,
        {
            method: 'POST'
        }
    )

    await validarResposta(resposta)
}

export async function recusarSelecao(
    selecaoId: number,
    motivo: string
): Promise<void> {
    const resposta = await fetch(
        `${API_URL}/selecoes/${selecaoId}/recusar`,
        {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ motivo })
        }
    )

    await validarResposta(resposta)
}

export async function listarHistoricoSelecao(
    selecaoId: number
): Promise<HistoricoSelecao[]> {
    const resposta = await fetch(
        `${API_URL}/selecoes/${selecaoId}/historico`
    )

    await validarResposta(resposta)
    return resposta.json()
}