import type {
    AtualizarClienteDados,
    Cliente,
    ClienteCadastrado,
    CriarClienteDados
} from '../types/Cliente'

const API_URL = 'http://localhost:5141/api/clientes'

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

export async function cadastrarCliente(
    dados: CriarClienteDados
): Promise<ClienteCadastrado> {
    const resposta = await fetch(API_URL, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(dados)
    })

    if (!resposta.ok) {
        throw new Error(await obterErro(resposta))
    }

    return resposta.json()
}

export async function buscarCliente(
    id: number
): Promise<Cliente> {
    const resposta = await fetch(`${API_URL}/${id}`)

    if (!resposta.ok) {
        throw new Error(await obterErro(resposta))
    }

    return resposta.json()
}

export async function atualizarCliente(
    id: number,
    dados: AtualizarClienteDados
): Promise<{ mensagem: string }> {
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