import { useState } from "react";
import type { FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { format } from "date-fns";
import { useSearchParams } from "react-router-dom";
import { toast } from "sonner";

import {
  aceitarSolicitacao,
  avaliarServico,
  concluirServico,
  criarSolicitacao,
  iniciarServico,
  listarHistorico,
  listarSolicitacoes,
  mensagemDeErro,
} from "@/services/solicitacoesApi";
import { listarTecnicos } from "@/services/tecnicosApi";
import type {
  EventoSolicitacao,
  PayloadAceite,
  PayloadAvaliacao,
  PayloadConclusao,
  PayloadInicio,
  Solicitacao,
} from "@/types/Solicitacao";

// Rótulos dos status — status futuros (de outros módulos) caem no texto bruto.
const ROTULOS_STATUS: Record<string, string> = {
  AguardandoAceite: "Aguardando aceite",
  Aceita: "Aceita",
  EmExecucao: "Em execução",
  Concluida: "Concluída",
  Avaliada: "Avaliada",
};

const CLASSES_STATUS: Record<string, string> = {
  AguardandoAceite: "aguardando",
  Aceita: "aceita",
  EmExecucao: "execucao",
  Concluida: "concluida",
  Avaliada: "avaliada",
};

const ROTULOS_EVENTO: Record<string, string> = {
  SolicitacaoCriada: "Solicitação criada",
  AceiteRegistrado: "Aceite registrado",
  ServicoIniciado: "Serviço iniciado",
  ServicoConcluido: "Serviço concluído",
  AvaliacaoRegistrada: "Avaliação registrada",
};

const STATUS_DO_FLUXO = Object.keys(ROTULOS_STATUS);

// Clientes de demonstração — substituídos pelo usuário autenticado quando o
// login (JWT) dos outros integrantes for integrado.
const CLIENTES_DEMONSTRACAO = [
  { id: 501, nome: "Ana Souza" },
  { id: 502, nome: "Bruno Lima" },
];

type Papel = "cliente" | "tecnico";
type TipoAcao = "aceite" | "conclusao" | "avaliacao";
type Acao = "aceite" | "iniciar" | "conclusao" | "avaliacao";

interface Ator {
  papel: Papel;
  id: number;
  nome: string;
}

function rotuloStatus(status: string): string {
  return ROTULOS_STATUS[status] ?? status;
}

function formatarData(valor: string): string {
  return format(new Date(valor), "dd/MM/yyyy 'às' HH:mm");
}

function formatarValor(valor: number): string {
  return valor.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });
}

// Mesmas regras do backend (routers/solicitacoes.py): quem age depende do status.
function acoesDisponiveis(solicitacao: Solicitacao, ator: Ator): Acao[] {
  if (ator.papel === "tecnico") {
    if (
      solicitacao.status === "AguardandoAceite" &&
      (solicitacao.tecnicoId === null || solicitacao.tecnicoId === ator.id)
    ) {
      return ["aceite"];
    }
    if (solicitacao.status === "Aceita" && solicitacao.tecnicoId === ator.id) {
      return ["iniciar"];
    }
    if (solicitacao.status === "EmExecucao" && solicitacao.tecnicoId === ator.id) {
      return ["conclusao"];
    }
    return [];
  }
  if (solicitacao.status === "Concluida" && solicitacao.clienteId === ator.id) {
    return ["avaliacao"];
  }
  return [];
}

function dicaSituacao(solicitacao: Solicitacao): string {
  switch (solicitacao.status) {
    case "AguardandoAceite":
      return solicitacao.tecnicoId === null
        ? "Aguardando o aceite de um técnico."
        : `Aguardando o aceite de ${solicitacao.tecnicoNome ?? "o técnico designado"}.`;
    case "Aceita":
      return `Serviço aceito por ${solicitacao.tecnicoNome ?? "o técnico"}. Aguardando início da execução.`;
    case "EmExecucao":
      return "Serviço em execução pelo técnico responsável.";
    case "Concluida":
      return "Serviço concluído. Aguardando avaliação do cliente.";
    case "Avaliada":
      return `Serviço avaliado com nota ${solicitacao.notaAvaliacao}/5. Fluxo encerrado.`;
    default:
      return ""; // status futuro registrado por outro módulo
  }
}

function LinhaDoTempo({ solicitacaoId }: { solicitacaoId: string }) {
  const {
    data: eventos,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["historico", solicitacaoId],
    queryFn: () => listarHistorico(solicitacaoId),
  });

  if (isLoading) {
    return <p className="dica-situacao">Carregando histórico...</p>;
  }

  if (error) {
    return <p className="mensagem erro">{mensagemDeErro(error)}</p>;
  }

  return (
    <ol className="linha-tempo" data-testid="linha-do-tempo">
      {(eventos ?? []).map((evento: EventoSolicitacao) => (
        <li className="evento-tempo" key={evento.id} data-testid="timeline-event">
          <h4>{ROTULOS_EVENTO[evento.tipoEvento] ?? evento.tipoEvento}</h4>
          <p>{evento.descricao}</p>
          <span className="evento-meta">
            {formatarData(evento.dataEvento)}
            {evento.autorPapel !== "sistema" && evento.autorId !== null
              ? ` · ${evento.autorPapel} #${evento.autorId}`
              : ""}
            {evento.statusNovo ? ` · status: ${rotuloStatus(evento.statusNovo)}` : ""}
          </span>
        </li>
      ))}
    </ol>
  );
}

export default function Solicitacoes() {
  const queryClient = useQueryClient();
  const [parametros] = useSearchParams();

  // Ator temporário (padrão do time até o login JWT chegar).
  const [papel, setPapel] = useState<Papel>("cliente");
  const [idCliente, setIdCliente] = useState(CLIENTES_DEMONSTRACAO[0].id);
  const tecnicoSolicitado = Number(parametros.get("tecnicoId")) || 0;
  const [idTecnico, setIdTecnico] = useState(tecnicoSolicitado);

  const [filtroStatus, setFiltroStatus] = useState("Todas");
  const [historicoAbertoId, setHistoricoAbertoId] = useState<string | null>(null);
  const [acaoAberta, setAcaoAberta] = useState<{ id: string; tipo: TipoAcao } | null>(null);

  // Nova solicitação
  const [equipamento, setEquipamento] = useState("");
  const [problemaRelatado, setProblemaRelatado] = useState("");
  const [tecnicoPreferido, setTecnicoPreferido] = useState(
    tecnicoSolicitado ? String(tecnicoSolicitado) : "",
  );

  // Campos das ações
  const [valorOrcamento, setValorOrcamento] = useState("");
  const [mensagemAceite, setMensagemAceite] = useState("");
  const [observacaoFinal, setObservacaoFinal] = useState("");
  const [nota, setNota] = useState(0);
  const [comentarioAvaliacao, setComentarioAvaliacao] = useState("");

  const {
    data: tecnicos = [],
    isLoading: carregandoTecnicos,
  } = useQuery({ queryKey: ["tecnicos"], queryFn: listarTecnicos });

  const {
    data: solicitacoes = [],
    isLoading: carregandoSolicitacoes,
    error: erroSolicitacoes,
  } = useQuery({ queryKey: ["solicitacoes"], queryFn: listarSolicitacoes });

  const idTecnicoEfetivo = idTecnico || tecnicos[0]?.id || 0;
  const tecnico = tecnicos.find((item) => item.id === idTecnicoEfetivo);
  const cliente = CLIENTES_DEMONSTRACAO.find((item) => item.id === idCliente);

  const ator: Ator | null =
    papel === "tecnico"
      ? tecnico
        ? { papel: "tecnico", id: tecnico.id, nome: tecnico.nomeExibicao }
        : null
      : cliente
        ? { papel: "cliente", id: cliente.id, nome: cliente.nome }
        : null;

  function fecharAcao() {
    setAcaoAberta(null);
    setValorOrcamento("");
    setMensagemAceite("");
    setObservacaoFinal("");
    setNota(0);
    setComentarioAvaliacao("");
  }

  function invalidarSolicitacoes() {
    queryClient.invalidateQueries({ queryKey: ["solicitacoes"] });
    queryClient.invalidateQueries({ queryKey: ["historico"] });
  }

  const criar = useMutation({
    mutationFn: criarSolicitacao,
    onSuccess: (criada) => {
      invalidarSolicitacoes();
      setEquipamento("");
      setProblemaRelatado("");
      setTecnicoPreferido("");
      toast.success(`Solicitação "${criada.equipamento}" registrada. Aguardando aceite do técnico.`);
    },
    onError: (erro) => toast.error(mensagemDeErro(erro)),
  });

  const aceitar = useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: PayloadAceite }) =>
      aceitarSolicitacao(id, dados),
    onSuccess: (atualizada) => {
      invalidarSolicitacoes();
      fecharAcao();
      toast.success(`Solicitação aceita por ${atualizada.tecnicoNome}.`);
    },
    onError: (erro) => toast.error(mensagemDeErro(erro)),
  });

  const iniciar = useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: PayloadInicio }) =>
      iniciarServico(id, dados),
    onSuccess: () => {
      invalidarSolicitacoes();
      toast.success("Serviço iniciado. Bom trabalho!");
    },
    onError: (erro) => toast.error(mensagemDeErro(erro)),
  });

  const concluir = useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: PayloadConclusao }) =>
      concluirServico(id, dados),
    onSuccess: () => {
      invalidarSolicitacoes();
      fecharAcao();
      toast.success("Serviço concluído. Aguardando avaliação do cliente.");
    },
    onError: (erro) => toast.error(mensagemDeErro(erro)),
  });

  const avaliar = useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: PayloadAvaliacao }) =>
      avaliarServico(id, dados),
    onSuccess: (atualizada) => {
      invalidarSolicitacoes();
      fecharAcao();
      toast.success(`Avaliação registrada com nota ${atualizada.notaAvaliacao}/5. Obrigado!`);
    },
    onError: (erro) => toast.error(mensagemDeErro(erro)),
  });

  function enviarNovaSolicitacao(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    if (!cliente) return;

    criar.mutate({
      clienteId: cliente.id,
      clienteNome: cliente.nome,
      tecnicoId: tecnicoPreferido ? Number(tecnicoPreferido) : null,
      equipamento,
      problemaRelatado,
    });
  }

  function enviarAceite(evento: FormEvent<HTMLFormElement>, solicitacao: Solicitacao) {
    evento.preventDefault();
    if (!ator) return;

    aceitar.mutate({
      id: solicitacao.id,
      dados: {
        tecnicoId: ator.id,
        valorOrcamento: valorOrcamento ? Number(valorOrcamento.replace(",", ".")) : undefined,
        mensagem: mensagemAceite.trim() ? mensagemAceite.trim() : undefined,
      },
    });
  }

  function enviarConclusao(evento: FormEvent<HTMLFormElement>, solicitacao: Solicitacao) {
    evento.preventDefault();
    if (!ator) return;

    concluir.mutate({
      id: solicitacao.id,
      dados: {
        tecnicoId: ator.id,
        observacaoFinal: observacaoFinal.trim() ? observacaoFinal.trim() : undefined,
      },
    });
  }

  function enviarAvaliacao(evento: FormEvent<HTMLFormElement>, solicitacao: Solicitacao) {
    evento.preventDefault();
    if (!ator || nota < 1) return;

    avaliar.mutate({
      id: solicitacao.id,
      dados: {
        clienteId: ator.id,
        nota,
        comentario: comentarioAvaliacao.trim() ? comentarioAvaliacao.trim() : undefined,
      },
    });
  }

  const visiveis =
    filtroStatus === "Todas"
      ? solicitacoes
      : solicitacoes.filter((solicitacao) => solicitacao.status === filtroStatus);

  const contagens: Record<string, number> = {};
  for (const solicitacao of solicitacoes) {
    contagens[solicitacao.status] = (contagens[solicitacao.status] ?? 0) + 1;
  }

  return (
    <>
      <header className="cabecalho-pagina">
        <div>
          <span className="subtitulo">Central de serviços</span>

          <h1>Solicitações de serviço</h1>

          <p>
            Acompanhe o aceite, a execução, a conclusão e a avaliação — com o
            histórico completo de eventos de cada solicitação.
          </p>
        </div>
      </header>

      <div className="barra-ator" data-testid="barra-ator">
        <div className="troca-papel">
          <button
            className={papel === "cliente" ? "ativo" : ""}
            data-testid="role-toggle-cliente"
            onClick={() => setPapel("cliente")}
          >
            Cliente
          </button>

          <button
            className={papel === "tecnico" ? "ativo" : ""}
            data-testid="role-toggle-tecnico"
            onClick={() => setPapel("tecnico")}
          >
            Técnico
          </button>
        </div>

        {papel === "cliente" ? (
          <label>
            Cliente em demonstração
            <select
              value={idCliente}
              onChange={(evento) => setIdCliente(Number(evento.target.value))}
              data-testid="select-cliente"
            >
              {CLIENTES_DEMONSTRACAO.map((item) => (
                <option key={item.id} value={item.id}>
                  {item.nome}
                </option>
              ))}
            </select>
          </label>
        ) : (
          <label>
            Técnico em demonstração
            <select
              value={idTecnicoEfetivo}
              onChange={(evento) => setIdTecnico(Number(evento.target.value))}
              data-testid="select-tecnico"
            >
              {tecnicos.map((item) => (
                <option key={item.id} value={item.id}>
                  {item.nomeExibicao}
                </option>
              ))}
            </select>
          </label>
        )}

        <small>
          Seletor temporário até a integração do login (JWT): as ações usam este
          ator.
        </small>
      </div>

      <main className="pagina-solicitacoes">
        {ator?.papel === "cliente" && (
          <section className="painel-nova-solicitacao" data-testid="painel-nova-solicitacao">
            <div className="cabecalho-formulario">
              <span className="subtitulo-formulario">Abrir solicitação</span>

              <h2>Descreva o problema do seu equipamento</h2>

              <p>A solicitação fica aguardando o aceite de um técnico.</p>
            </div>

            <form
              className="formulario"
              data-testid="form-nova-solicitacao"
              onSubmit={enviarNovaSolicitacao}
            >
              <label>
                Equipamento
                <input
                  type="text"
                  required
                  placeholder="Ex.: Geladeira Brastemp frost free"
                  value={equipamento}
                  onChange={(evento) => setEquipamento(evento.target.value)}
                  data-testid="input-equipamento"
                />
              </label>

              <label>
                Problema relatado
                <textarea
                  rows={3}
                  required
                  placeholder="Descreva o que está acontecendo."
                  value={problemaRelatado}
                  onChange={(evento) => setProblemaRelatado(evento.target.value)}
                  data-testid="textarea-problema-relatado"
                />
              </label>

              <label>
                Técnico preferido (opcional)
                <select
                  value={tecnicoPreferido}
                  onChange={(evento) => setTecnicoPreferido(evento.target.value)}
                  data-testid="select-tecnico-preferido"
                >
                  <option value="">Definido pelo aceite do técnico</option>
                  {tecnicos.map((item) => (
                    <option key={item.id} value={item.id}>
                      {item.nomeExibicao} — {item.cidadeAtendimento}/
                      {item.estadoAtendimento}
                    </option>
                  ))}
                </select>
                <small>
                  O módulo de matching futuro poderá designar o técnico
                  automaticamente.
                </small>
              </label>

              {criar.error && <p className="mensagem erro">{mensagemDeErro(criar.error)}</p>}

              <button
                className="botao-principal"
                type="submit"
                disabled={criar.isPending}
                data-testid="btn-criar-solicitacao"
              >
                {criar.isPending ? "Registrando..." : "Registrar solicitação"}
              </button>
            </form>
          </section>
        )}

        <section className="filtros-status" data-testid="filtros-status">
          <button
            className={filtroStatus === "Todas" ? "filtro-status ativo" : "filtro-status"}
            data-testid="filtro-status-todas"
            onClick={() => setFiltroStatus("Todas")}
          >
            Todas <span className="contagem">{solicitacoes.length}</span>
          </button>

          {STATUS_DO_FLUXO.map((status) => (
            <button
              key={status}
              className={filtroStatus === status ? "filtro-status ativo" : "filtro-status"}
              data-testid={`filtro-status-${status.toLowerCase()}`}
              onClick={() => setFiltroStatus(status)}
            >
              {rotuloStatus(status)} <span className="contagem">{contagens[status] ?? 0}</span>
            </button>
          ))}
        </section>

        <section className="lista-solicitacoes" data-testid="lista-solicitacoes">
          {carregandoSolicitacoes && <p>Carregando solicitações...</p>}

          {erroSolicitacoes && (
            <p className="mensagem erro">{mensagemDeErro(erroSolicitacoes)}</p>
          )}

          {!carregandoSolicitacoes && !erroSolicitacoes && visiveis.length === 0 && (
            <p>Nenhuma solicitação encontrada.</p>
          )}

          {visiveis.map((solicitacao) => {
            const acoes = ator ? acoesDisponiveis(solicitacao, ator) : [];

            return (
              <article
                className="cartao-solicitacao"
                key={solicitacao.id}
                data-testid="solicitacao-card"
              >
                <div className="cabecalho-cartao-solicitacao">
                  <h2>{solicitacao.equipamento}</h2>

                  <span
                    className={`pill-status ${CLASSES_STATUS[solicitacao.status] ?? "desconhecido"}`}
                    data-testid={`status-solicitacao-${solicitacao.id}`}
                  >
                    {rotuloStatus(solicitacao.status)}
                  </span>
                </div>

                <p className="descricao-solicitacao">{solicitacao.problemaRelatado}</p>

                <div className="meta-solicitacao">
                  <span>
                    Cliente: <strong>{solicitacao.clienteNome}</strong>
                  </span>

                  <span>
                    Técnico:{" "}
                    <strong>{solicitacao.tecnicoNome ?? "Não designado"}</strong>
                  </span>

                  {solicitacao.valorOrcamento !== null && (
                    <span className="valor-orcamento">
                      Orçamento: <strong>{formatarValor(solicitacao.valorOrcamento)}</strong>
                    </span>
                  )}

                  {solicitacao.notaAvaliacao !== null && (
                    <span>
                      Avaliação: <strong>{"★".repeat(solicitacao.notaAvaliacao)}</strong>
                    </span>
                  )}

                  <span>
                    Aberta em <strong>{formatarData(solicitacao.dataSolicitacao)}</strong>
                  </span>
                </div>

                <p className="dica-situacao">{dicaSituacao(solicitacao)}</p>

                <div className="acoes-solicitacao">
                  {ator && acoes.includes("aceite") && (
                    <button
                      className="botao-principal"
                      data-testid="btn-aceitar-solicitacao"
                      onClick={() =>
                        setAcaoAberta(
                          acaoAberta?.id === solicitacao.id && acaoAberta.tipo === "aceite"
                            ? null
                            : { id: solicitacao.id, tipo: "aceite" },
                        )
                      }
                    >
                      {acaoAberta?.id === solicitacao.id && acaoAberta.tipo === "aceite"
                        ? "Cancelar aceite"
                        : "Aceitar solicitação"}
                    </button>
                  )}

                  {ator && acoes.includes("iniciar") && (
                    <button
                      className="botao-principal"
                      data-testid="btn-iniciar-servico"
                      disabled={iniciar.isPending}
                      onClick={() =>
                        iniciar.mutate({
                          id: solicitacao.id,
                          dados: { tecnicoId: ator.id },
                        })
                      }
                    >
                      {iniciar.isPending ? "Iniciando..." : "Iniciar serviço"}
                    </button>
                  )}

                  {ator && acoes.includes("conclusao") && (
                    <button
                      className="botao-principal"
                      data-testid="btn-concluir-servico"
                      onClick={() =>
                        setAcaoAberta(
                          acaoAberta?.id === solicitacao.id && acaoAberta.tipo === "conclusao"
                            ? null
                            : { id: solicitacao.id, tipo: "conclusao" },
                        )
                      }
                    >
                      Concluir serviço
                    </button>
                  )}

                  {ator && acoes.includes("avaliacao") && (
                    <button
                      className="botao-principal"
                      data-testid="btn-abrir-avaliacao"
                      onClick={() =>
                        setAcaoAberta(
                          acaoAberta?.id === solicitacao.id && acaoAberta.tipo === "avaliacao"
                            ? null
                            : { id: solicitacao.id, tipo: "avaliacao" },
                        )
                      }
                    >
                      Avaliar serviço
                    </button>
                  )}

                  <button
                    className="botao-secundario"
                    data-testid={`btn-historico-${solicitacao.id}`}
                    onClick={() =>
                      setHistoricoAbertoId(
                        historicoAbertoId === solicitacao.id ? null : solicitacao.id,
                      )
                    }
                  >
                    {historicoAbertoId === solicitacao.id
                      ? "Ocultar histórico"
                      : "Ver histórico"}
                  </button>
                </div>

                {acaoAberta?.id === solicitacao.id && acaoAberta.tipo === "aceite" && (
                  <form
                    className="formulario-acao"
                    data-testid="form-aceite"
                    onSubmit={(evento) => enviarAceite(evento, solicitacao)}
                  >
                    <label>
                      Valor do orçamento (opcional)
                      <input
                        type="number"
                        min="1"
                        step="0.01"
                        placeholder="Ex.: 350,00"
                        value={valorOrcamento}
                        onChange={(evento) => setValorOrcamento(evento.target.value)}
                        data-testid="input-valor-orcamento"
                      />
                      <small>
                        Opcional agora — o futuro módulo de orçamento poderá
                        preencher este campo.
                      </small>
                    </label>

                    <label>
                      Mensagem (opcional)
                      <textarea
                        rows={2}
                        value={mensagemAceite}
                        onChange={(evento) => setMensagemAceite(evento.target.value)}
                        data-testid="textarea-mensagem-aceite"
                      />
                    </label>

                    <div className="acoes-formulario">
                      <button
                        className="botao-principal"
                        type="submit"
                        disabled={aceitar.isPending}
                        data-testid="btn-confirmar-aceite"
                      >
                        {aceitar.isPending ? "Registrando..." : "Confirmar aceite"}
                      </button>

                      <button className="botao-secundario" type="button" onClick={fecharAcao}>
                        Cancelar
                      </button>
                    </div>
                  </form>
                )}

                {acaoAberta?.id === solicitacao.id && acaoAberta.tipo === "conclusao" && (
                  <form
                    className="formulario-acao"
                    data-testid="form-conclusao"
                    onSubmit={(evento) => enviarConclusao(evento, solicitacao)}
                  >
                    <label>
                      Observações finais (opcional)
                      <textarea
                        rows={3}
                        placeholder="O que foi feito no atendimento."
                        value={observacaoFinal}
                        onChange={(evento) => setObservacaoFinal(evento.target.value)}
                        data-testid="textarea-observacao-final"
                      />
                    </label>

                    <div className="acoes-formulario">
                      <button
                        className="botao-principal"
                        type="submit"
                        disabled={concluir.isPending}
                        data-testid="btn-confirmar-conclusao"
                      >
                        {concluir.isPending ? "Concluindo..." : "Confirmar conclusão"}
                      </button>

                      <button className="botao-secundario" type="button" onClick={fecharAcao}>
                        Cancelar
                      </button>
                    </div>
                  </form>
                )}

                {acaoAberta?.id === solicitacao.id && acaoAberta.tipo === "avaliacao" && (
                  <form
                    className="formulario-acao"
                    data-testid="form-avaliacao"
                    onSubmit={(evento) => enviarAvaliacao(evento, solicitacao)}
                  >
                    <label>
                      Sua nota
                      <div className="estrelas" role="radiogroup" aria-label="Nota do serviço">
                        {[1, 2, 3, 4, 5].map((valor) => (
                          <button
                            key={valor}
                            type="button"
                            className={valor <= nota ? "estrela marcada" : "estrela"}
                            data-testid={`estrela-nota-${valor}`}
                            aria-label={`${valor} de 5`}
                            onClick={() => setNota(valor)}
                          >
                            ★
                          </button>
                        ))}
                      </div>
                    </label>

                    <label>
                      Comentário (opcional)
                      <textarea
                        rows={3}
                        value={comentarioAvaliacao}
                        onChange={(evento) => setComentarioAvaliacao(evento.target.value)}
                        data-testid="textarea-comentario-avaliacao"
                      />
                    </label>

                    <div className="acoes-formulario">
                      <button
                        className="botao-principal"
                        type="submit"
                        disabled={avaliar.isPending || nota < 1}
                        data-testid="btn-enviar-avaliacao"
                      >
                        {avaliar.isPending ? "Enviando..." : "Enviar avaliação"}
                      </button>

                      <button className="botao-secundario" type="button" onClick={fecharAcao}>
                        Cancelar
                      </button>
                    </div>
                  </form>
                )}

                {historicoAbertoId === solicitacao.id && (
                  <LinhaDoTempo solicitacaoId={solicitacao.id} />
                )}
              </article>
            );
          })}
        </section>

        {carregandoTecnicos && papel === "tecnico" && (
          <p className="dica-situacao">Carregando técnicos...</p>
        )}
      </main>
    </>
  );
}
