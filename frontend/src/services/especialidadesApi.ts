import type {
  CriarEspecialidade,
  Especialidade
} from '../types/Especialidade'

const API_URL =
  'http://localhost:5141/api/especialidades'

export async function listarEspecialidades():
Promise<Especialidade[]> {
  const resposta = await fetch(API_URL)

  if (!resposta.ok) {
    throw new Error(
      'Não foi possível carregar as especialidades.'
    )
  }

  return resposta.json()
}

export async function cadastrarEspecialidade(
  dados: CriarEspecialidade
): Promise<Especialidade> {
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
      mensagem ||
      'Não foi possível cadastrar a especialidade.'
    )
  }

  return resposta.json()
}

export async function atualizarEspecialidade(
  especialidadeId: number,
  dados: CriarEspecialidade
): Promise<Especialidade> {
  const resposta = await fetch(
    `${API_URL}/${especialidadeId}`,
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
      'Não foi possível atualizar a especialidade.'
    )
  }

  return resposta.json()
}

export async function alterarStatusEspecialidade(
  especialidadeId: number,
  ativar: boolean
): Promise<Especialidade> {
  const acao = ativar ? 'ativar' : 'inativar'

  const resposta = await fetch(
    `${API_URL}/${especialidadeId}/${acao}`,
    {
      method: 'PATCH'
    }
  )

  if (!resposta.ok) {
    const mensagem = await resposta.text()

    throw new Error(
      mensagem ||
      'Não foi possível alterar o status da especialidade.'
    )
  }

  return resposta.json()
}