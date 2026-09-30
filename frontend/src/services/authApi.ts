import type {
    LoginDados,
    SessaoUsuario
} from '../types/Sessao'

const API_URL = 'http://localhost:5141/api/auth'

async function obterErro(
    resposta: Response
): Promise<string> {
    const texto = await resposta.text()

    if (!texto) {
        return 'Não foi possível realizar o login.'
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

export async function realizarLogin(
    dados: LoginDados
): Promise<SessaoUsuario> {
    const resposta = await fetch(`${API_URL}/login`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(dados)
    })

    if (!resposta.ok) {
        throw new Error(await obterErro(resposta))
    }

    const usuario: SessaoUsuario =
        await resposta.json()

    localStorage.setItem(
        'sessaoUsuario',
        JSON.stringify(usuario)
    )

    return usuario
}

export function obterSessao():
    SessaoUsuario | null {
    const conteudo = localStorage.getItem(
        'sessaoUsuario'
    )

    if (!conteudo) {
        return null
    }

    try {
        return JSON.parse(conteudo)
    } catch {
        localStorage.removeItem('sessaoUsuario')
        return null
    }
}

export function encerrarSessao() {
    localStorage.removeItem('sessaoUsuario')
}