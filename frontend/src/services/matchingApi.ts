import type {
  SelecaoTecnico,
  SelecionarTecnicoDados,
  TecnicoCompativel
} from '../types/Matching'

const API_URL = 'http://localhost:5141/api/matching'

async function obterMensagemErro(
  resposta: Response
): Promise<string> {
  const texto = await resposta.text()

  if (!texto) {
    return 'Não foi possível concluir a operação.'
  }

  try {
    const dados = JSON.parse(texto)

    return (
      dados.message ||
      dados.title ||
      texto
    )
  } catch {
    return texto
  }
}

export async function buscarTecnicosCompativeis(
  especialidadeId: number,
  cidade: string
): Promise<TecnicoCompativel[]> {
  const parametros = new URLSearchParams({
    especialidadeId: especialidadeId.toString()
  })

  if (cidade.trim()) {
    parametros.append('cidade', cidade.trim())
  }

  const resposta = await fetch(
    `${API_URL}/tecnicos?${parametros.toString()}`
  )

  if (!resposta.ok) {
    throw new Error(
      await obterMensagemErro(resposta)
    )
  }

  return resposta.json()
}

export async function selecionarTecnico(
  dados: SelecionarTecnicoDados
): Promise<SelecaoTecnico> {
  const resposta = await fetch(
    `${API_URL}/selecionar`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(dados)
    }
  )

  if (!resposta.ok) {
    throw new Error(
      await obterMensagemErro(resposta)
    )
  }

  return resposta.json()
}

export async function buscarSelecaoDaSolicitacao(
  solicitacaoId: number
): Promise<SelecaoTecnico> {
  const resposta = await fetch(
    `${API_URL}/solicitacoes/${solicitacaoId}`
  )

  if (!resposta.ok) {
    throw new Error(
      await obterMensagemErro(resposta)
    )
  }

  return resposta.json()
}