import type {
  AdicionarEspecialidadeTecnico,
  AtualizarExperienciaTecnico,
  AtualizarTecnico,
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

export async function atualizarExperienciaTecnico(
  tecnicoId: number,
  especialidadeId: number,
  dados: AtualizarExperienciaTecnico
): Promise<void> {
  const resposta = await fetch(
    `${API_URL}/${tecnicoId}/especialidades/${especialidadeId}`,
    {
      method: 'PUT',
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
      'Não foi possível atualizar a experiência.'
    )
  }
}

export async function atualizarTecnico(
  tecnicoId: number,
  dados: AtualizarTecnico
): Promise<void> {
  const resposta = await fetch(
    `${API_URL}/${tecnicoId}`,
    {
      method: 'PUT',
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
      'Não foi possível atualizar o técnico.'
    )
  }
}

export async function alterarStatusTecnico(
  tecnicoId: number,
  ativar: boolean
): Promise<void> {
  const acao = ativar ? 'ativar' : 'inativar'

  const resposta = await fetch(
    `${API_URL}/${tecnicoId}/${acao}`,
    {
      method: 'PATCH'
    }
  )

  if (!resposta.ok) {
    const mensagem = await resposta.text()

    throw new Error(
      mensagem ||
      'Não foi possível alterar o status do técnico.'
    )
  }
}

export async function desvincularEspecialidadeTecnico(
  tecnicoId: number,
  especialidadeId: number
): Promise<void> {
  const resposta = await fetch(
    `${API_URL}/${tecnicoId}/especialidades/${especialidadeId}`,
    {
      method: 'DELETE'
    }
  )

  if (!resposta.ok) {
    const mensagem = await resposta.text()

    throw new Error(
      mensagem ||
      'Não foi possível remover a especialidade.'
    )
  }
}