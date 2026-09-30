import type {
    Equipamento,
    EquipamentoDados
} from '../types/Equipamento'

const API_URL =
    'http://localhost:5141/api/equipamentos'

async function obterErro(
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

export async function listarEquipamentos(
    clienteId: number
): Promise<Equipamento[]> {
    const resposta = await fetch(
        `${API_URL}/cliente/${clienteId}`
    )

    if (!resposta.ok) {
        throw new Error(await obterErro(resposta))
    }

    return resposta.json()
}

export async function cadastrarEquipamento(
    clienteId: number,
    dados: EquipamentoDados
): Promise<Equipamento> {
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

export async function atualizarEquipamento(
    id: number,
    dados: EquipamentoDados
): Promise<Equipamento> {
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

    if (!resposta.ok) {
        throw new Error(await obterErro(resposta))
    }

    return resposta.json()
}

export async function excluirEquipamento(
    id: number
): Promise<void> {
    const resposta = await fetch(
        `${API_URL}/${id}`,
        {
            method: 'DELETE'
        }
    )

    if (!resposta.ok) {
        throw new Error(await obterErro(resposta))
    }
}