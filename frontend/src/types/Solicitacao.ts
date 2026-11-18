// Espelho dos contratos JSON do módulo de solicitações (backend/models/solicitacoes.py) — manter em sincronia.
export interface Solicitacao {
  id: string;
  clienteId: number;
  clienteNome: string;
  tecnicoId: number | null;
  tecnicoNome: string | null;
  equipamento: string;
  problemaRelatado: string;
  valorOrcamento: number | null;
  status: string;
  dataSolicitacao: string;
  dataAceite: string | null;
  dataInicio: string | null;
  dataConclusao: string | null;
  observacaoFinal: string | null;
  notaAvaliacao: number | null;
  comentarioAvaliacao: string | null;
  dataAvaliacao: string | null;
}

export interface EventoSolicitacao {
  id: string;
  solicitacaoId: string;
  tipoEvento: string;
  statusAnterior: string | null;
  statusNovo: string | null;
  descricao: string;
  autorId: number | null;
  autorPapel: string;
  dataEvento: string;
}

export interface CriarSolicitacao {
  clienteId: number;
  clienteNome: string;
  tecnicoId: number | null;
  equipamento: string;
  problemaRelatado: string;
}

export interface PayloadAceite {
  tecnicoId: number;
  valorOrcamento?: number;
  mensagem?: string;
}

export interface PayloadInicio {
  tecnicoId: number;
  observacao?: string;
}

export interface PayloadConclusao {
  tecnicoId: number;
  observacaoFinal?: string;
}

export interface PayloadAvaliacao {
  clienteId: number;
  nota: number;
  comentario?: string;
}
