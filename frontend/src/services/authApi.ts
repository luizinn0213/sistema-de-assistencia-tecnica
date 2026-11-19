import type {
    LoginDados,
    RespostaAutenticacao,
    SessaoUsuario
} from '../types/Sessao'

const API_URL = 'http://localhost:5141/api/auth'
const CHAVE_SESSAO = 'sessaoUsuario'
const CHAVE_ACCESS = 'accessToken'
const CHAVE_REFRESH = 'refreshToken'

async function obterErro(resposta: Response): Promise<string> {
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

function armazenarAutenticacao(dados: RespostaAutenticacao) {
    localStorage.setItem(CHAVE_ACCESS, dados.accessToken)
    localStorage.setItem(CHAVE_REFRESH, dados.refreshToken)
    localStorage.setItem(CHAVE_SESSAO, JSON.stringify(dados.usuario))
}

function limparAutenticacao() {
    localStorage.removeItem(CHAVE_ACCESS)
    localStorage.removeItem(CHAVE_REFRESH)
    localStorage.removeItem(CHAVE_SESSAO)
}

export async function realizarLogin(
    dados: LoginDados
): Promise<SessaoUsuario> {
    const resposta = await fetch(`${API_URL}/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(dados)
    })

    if (!resposta.ok) {
        throw new Error(await obterErro(resposta))
    }

    const autenticacao: RespostaAutenticacao = await resposta.json()
    armazenarAutenticacao(autenticacao)
    return autenticacao.usuario
}

export function obterSessao(): SessaoUsuario | null {
    const conteudo = localStorage.getItem(CHAVE_SESSAO)

    if (!conteudo) return null

    try {
        return JSON.parse(conteudo)
    } catch {
        limparAutenticacao()
        return null
    }
}

let renovacaoEmAndamento: Promise<string> | null = null

async function renovarAccessToken(): Promise<string> {
    const refreshToken = localStorage.getItem(CHAVE_REFRESH)

    if (!refreshToken) {
        throw new Error('Sessão expirada. Faça login novamente.')
    }

    const resposta = await fetch(`${API_URL}/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken })
    })

    if (!resposta.ok) {
        limparAutenticacao()
        throw new Error(await obterErro(resposta))
    }

    const autenticacao: RespostaAutenticacao = await resposta.json()
    armazenarAutenticacao(autenticacao)
    return autenticacao.accessToken
}

export async function fetchAutenticado(
    entrada: RequestInfo | URL,
    opcoes: RequestInit = {}
): Promise<Response> {
    const headers = new Headers(opcoes.headers)
    const accessToken = localStorage.getItem(CHAVE_ACCESS)

    if (accessToken) {
        headers.set('Authorization', `Bearer ${accessToken}`)
    }

    let resposta = await fetch(entrada, { ...opcoes, headers })

    if (resposta.status !== 401 || !localStorage.getItem(CHAVE_REFRESH)) {
        return resposta
    }

    renovacaoEmAndamento ??= renovarAccessToken().finally(() => {
        renovacaoEmAndamento = null
    })

    try {
        const novoToken = await renovacaoEmAndamento
        headers.set('Authorization', `Bearer ${novoToken}`)
        resposta = await fetch(entrada, { ...opcoes, headers })
        return resposta
    } catch {
        limparAutenticacao()
        return resposta
    }
}

export async function obterUsuarioAtual(): Promise<SessaoUsuario> {
    const resposta = await fetchAutenticado(`${API_URL}/eu`)

    if (!resposta.ok) {
        throw new Error(await obterErro(resposta))
    }

    const usuario: SessaoUsuario = await resposta.json()
    localStorage.setItem(CHAVE_SESSAO, JSON.stringify(usuario))
    return usuario
}

export async function trocarSenha(
    senhaAtual: string,
    novaSenha: string
): Promise<string> {
    const resposta = await fetchAutenticado(`${API_URL}/senha`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ senhaAtual, novaSenha })
    })

    if (!resposta.ok) {
        throw new Error(await obterErro(resposta))
    }

    const dados = await resposta.json()
    limparAutenticacao()
    return dados.mensagem
}

export async function encerrarSessao(): Promise<void> {
    try {
        const temToken = localStorage.getItem(CHAVE_ACCESS) ||
            localStorage.getItem(CHAVE_REFRESH)

        if (!temToken) return

        const resposta = await fetchAutenticado(`${API_URL}/logout`, {
            method: 'POST'
        })

        if (!resposta.ok) {
            throw new Error(await obterErro(resposta))
        }
    } finally {
        limparAutenticacao()
    }
}
