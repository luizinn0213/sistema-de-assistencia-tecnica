export interface SessaoUsuario {
    id: number
    nome: string
    email: string
    tipo: string
    clienteId?: number
}

export interface LoginDados {
    email: string
    senha: string
}

export interface RespostaAutenticacao {
    accessToken: string
    tokenType: string
    accessTokenExpiraEm: string
    refreshToken: string
    refreshTokenExpiraEm: string
    usuario: SessaoUsuario
}
