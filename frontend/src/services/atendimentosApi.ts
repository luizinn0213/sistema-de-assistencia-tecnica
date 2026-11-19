import { fetchAutenticado } from './authApi'
import type { Atendimento } from '../types/Atendimento'

export async function listarAtendimentos(sinal: AbortSignal): Promise<Atendimento[]> {
    const resposta = await fetchAutenticado('http://localhost:5141/api/atendimentos', {
        signal: sinal
    })

    if (!resposta.ok) {
        throw new Error(resposta.status === 401
            ? 'Sua sessão expirou. Entre na sua conta novamente.'
            : 'Não foi possível carregar seus atendimentos.')
    }

    return resposta.json()
}
