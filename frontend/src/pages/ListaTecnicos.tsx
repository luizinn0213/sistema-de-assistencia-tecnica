import { useMemo } from "react";
import { Link } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";

import { listarSolicitacoes, mensagemDeErro } from "@/services/solicitacoesApi";
import { listarTecnicos } from "@/services/tecnicosApi";

export default function ListaTecnicos() {
  // A casca da página (hero, grade) renderiza sempre; só a região de dados
  // depende do fetch — sem backend a página continua de pé, nunca branca.
  const { data: tecnicos = [], isLoading, error } = useQuery({
    queryKey: ["tecnicos"],
    queryFn: listarTecnicos,
  });

  // Avaliações reais calculadas das solicitações já avaliadas no fluxo de serviços.
  const { data: solicitacoes = [] } = useQuery({
    queryKey: ["solicitacoes"],
    queryFn: listarSolicitacoes,
  });

  const avaliacoes = useMemo(() => {
    const porTecnico = new Map<number, { soma: number; total: number }>();
    for (const solicitacao of solicitacoes) {
      if (solicitacao.tecnicoId === null || solicitacao.notaAvaliacao === null) continue;
      const atual = porTecnico.get(solicitacao.tecnicoId) ?? { soma: 0, total: 0 };
      atual.soma += solicitacao.notaAvaliacao;
      atual.total += 1;
      porTecnico.set(solicitacao.tecnicoId, atual);
    }
    return porTecnico;
  }, [solicitacoes]);

  return (
    <main className="pagina-tecnicos">
      <header className="cabecalho-pagina">
        <div>
          <span className="subtitulo">Assistência técnica confiável</span>

          <h1>Encontre o técnico certo para seu equipamento</h1>

          <p>Consulte profissionais, especialidades e disponibilidade.</p>
        </div>
      </header>

      <section className="grade-tecnicos" data-testid="grade-tecnicos">
        {isLoading && <p>Carregando técnicos...</p>}

        {error && <p className="mensagem erro">{mensagemDeErro(error)}</p>}

        {!isLoading && !error && tecnicos.length === 0 && (
          <p>Nenhum técnico cadastrado.</p>
        )}

        {tecnicos.map((tecnico) => {
          const avaliacao = avaliacoes.get(tecnico.id);

          return (
            <article className="cartao-tecnico" key={tecnico.id} data-testid="cartao-tecnico">
              <div className="avatar">{tecnico.nomeExibicao.charAt(0).toUpperCase()}</div>

              <div className="dados-tecnico">
                <div className="titulo-cartao">
                  <h2>{tecnico.nomeExibicao}</h2>

                  <span
                    className={
                      tecnico.disponivel ? "status disponivel" : "status indisponivel"
                    }
                  >
                    {tecnico.disponivel ? "Disponível" : "Indisponível"}
                  </span>
                </div>

                <p>{tecnico.descricaoProfissional}</p>

                <p className="localizacao">
                  {tecnico.cidadeAtendimento} - {tecnico.estadoAtendimento}
                </p>

                {avaliacao && avaliacao.total > 0 && (
                  <p className="avaliacao-tecnico" data-testid="avaliacao-tecnico">
                    ★ {(avaliacao.soma / avaliacao.total).toFixed(1).replace(".", ",")} (
                    {avaliacao.total}{" "}
                    {avaliacao.total === 1 ? "avaliação" : "avaliações"})
                  </p>
                )}

                <div className="especialidades">
                  {tecnico.especialidades.length === 0 && (
                    <span>Sem especialidades cadastradas</span>
                  )}

                  {tecnico.especialidades.map((especialidade) => (
                    <span className="etiqueta" key={especialidade.especialidadeId}>
                      {especialidade.nome} · {especialidade.nivelExperiencia}
                    </span>
                  ))}
                </div>

                <div className="acoes-cartao">
                  <Link
                    className="botao-secundario"
                    to={`/solicitacoes?tecnicoId=${tecnico.id}`}
                    data-testid={`btn-solicitar-servico-${tecnico.id}`}
                  >
                    Solicitar serviço
                  </Link>
                </div>
              </div>
            </article>
          );
        })}
      </section>
    </main>
  );
}
