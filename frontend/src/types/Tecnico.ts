// Espelho do contrato JSON do módulo .NET da equipe (controller api/tecnicos) — manter em sincronia.
export interface EspecialidadeTecnico {
  especialidadeId: number;
  nome: string;
  nivelExperiencia: string;
  anosExperiencia: number;
}

export interface Tecnico {
  id: number;
  usuarioId: number;
  nomeExibicao: string;
  descricaoProfissional: string;
  cidadeAtendimento: string;
  estadoAtendimento: string;
  ativo: boolean;
  disponivel: boolean;
  dataCadastro: string;
  especialidades: EspecialidadeTecnico[];
}
