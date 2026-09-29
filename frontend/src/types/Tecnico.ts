export interface EspecialidadeTecnico {
  especialidadeId: number
  nome: string
  nivelExperiencia: string
  anosExperiencia: number
}

export interface Tecnico {
  id: number
  usuarioId: number
  nomeExibicao: string
  descricaoProfissional: string
  cidadeAtendimento: string
  estadoAtendimento: string
  ativo: boolean
  disponivel: boolean
  dataCadastro: string
  especialidades: EspecialidadeTecnico[]
}

export interface CriarTecnico {
  usuarioId: number
  nomeExibicao: string
  descricaoProfissional: string
  cidadeAtendimento: string
  estadoAtendimento: string
}

export interface AdicionarEspecialidadeTecnico {
  especialidadeId: number
  nivelExperiencia: number
  anosExperiencia: number
}