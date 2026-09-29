export interface Especialidade {
  id: number
  nome: string
  descricao: string
  ativa: boolean
}

export interface CriarEspecialidade {
  nome: string
  descricao: string
}