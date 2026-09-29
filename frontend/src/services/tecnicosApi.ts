import type { Tecnico } from '../types/Tecnico'

const API_URL = 'http://localhost:5141/api/tecnicos'

export async function listarTecnicos(): Promise<Tecnico[]> {
  const resposta = await fetch(API_URL)

  if (!resposta.ok) {
    throw new Error('Não foi possível carregar os técnicos.')
  }

  return resposta.json()
}