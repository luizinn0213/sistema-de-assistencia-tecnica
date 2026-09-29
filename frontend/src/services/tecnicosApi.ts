import type {
  AdicionarEspecialidadeTecnico,
  CriarTecnico,
  Tecnico
} from '../types/Tecnico'

const API_URL = 'http://localhost:5141/api/tecnicos'

export async function listarTecnicos(): Promise<Tecnico[]> {
  const resposta = await fetch(API_URL)

  if (!resposta.ok) {
    throw new Error('Não foi possível carregar os técnicos.')
  }

  return resposta.json()
}

export async function cadastrarTecnico(
  dados: CriarTecnico
): Promise<Tecnico> {
  const resposta = await fetch(API_URL, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(dados)
  })

  if (!resposta.ok) {
    const mensagem = await resposta.text()

    throw new Error(
      mensagem || 'Não foi possível cadastrar o técnico.'
    )
  }

  return resposta.json()
}

export async function adicionarEspecialidadeTecnico(
  tecnicoId: number,
  dados: AdicionarEspecialidadeTecnico
): Promise<void> {
  const resposta = await fetch(
    `${API_URL}/${tecnicoId}/especialidades`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(dados)
    }
  )

  if (!resposta.ok) {
    const mensagem = await resposta.text()

    throw new Error(
      mensagem ||
      'Não foi possível vincular a especialidade.'
    )
  }
}

export async function alterarDisponibilidadeTecnico(
  tecnicoId: number,
  disponivel: boolean
): Promise<void> {
  const resposta = await fetch(
    `${API_URL}/${tecnicoId}/disponibilidade`,
    {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ disponivel })
    }
  )

  if (!resposta.ok) {
    const mensagem = await resposta.text()

    throw new Error(
      mensagem ||
      'Não foi possível alterar a disponibilidade.'
    )
  }
}