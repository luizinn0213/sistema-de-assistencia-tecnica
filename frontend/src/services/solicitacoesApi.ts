import { ApiError, apiGet, apiPatch, apiPost } from "@/lib/api";
import type {
  CriarSolicitacao,
  EventoSolicitacao,
  PayloadAceite,
  PayloadConclusao,
  PayloadAvaliacao,
  PayloadInicio,
  Solicitacao,
} from "@/types/Solicitacao";

// Extrai a mensagem do backend: "detail" pode ser texto (400/404) ou lista de validação (422).
export function mensagemDeErro(erro: unknown): string {
  if (erro instanceof ApiError) {
    const detalhe = (erro.body as { detail?: unknown } | null)?.detail;
    if (typeof detalhe === "string" && detalhe.length > 0) return detalhe;
    if (detalhe) return "Verifique os dados informados e tente novamente.";
  }
  if (erro instanceof Error && erro.message) return erro.message;
  return "Não foi possível concluir a operação.";
}

export const listarSolicitacoes = () => apiGet<Solicitacao[]>("/solicitacoes");

export const listarHistorico = (id: string) =>
  apiGet<EventoSolicitacao[]>(`/solicitacoes/${id}/historico`);

export const criarSolicitacao = (dados: CriarSolicitacao) =>
  apiPost<Solicitacao>("/solicitacoes", dados);

export const aceitarSolicitacao = (id: string, dados: PayloadAceite) =>
  apiPatch<Solicitacao>(`/solicitacoes/${id}/aceitar`, dados);

export const iniciarServico = (id: string, dados: PayloadInicio) =>
  apiPatch<Solicitacao>(`/solicitacoes/${id}/iniciar`, dados);

export const concluirServico = (id: string, dados: PayloadConclusao) =>
  apiPatch<Solicitacao>(`/solicitacoes/${id}/concluir`, dados);

export const avaliarServico = (id: string, dados: PayloadAvaliacao) =>
  apiPost<Solicitacao>(`/solicitacoes/${id}/avaliar`, dados);
