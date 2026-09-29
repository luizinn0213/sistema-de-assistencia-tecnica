import { useEffect, useState } from 'react'
import { listarTecnicos } from '../services/tecnicosApi'
import type { Tecnico } from '../types/Tecnico'

export function ListaTecnicos() {
  const [tecnicos, setTecnicos] = useState<Tecnico[]>([])
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')

  useEffect(() => {
    let componenteAtivo = true

    listarTecnicos()
      .then((dados) => {
        if (componenteAtivo) {
          setTecnicos(dados)
        }
      })
      .catch(() => {
        if (componenteAtivo) {
          setErro('Não foi possível carregar os técnicos.')
        }
      })
      .finally(() => {
        if (componenteAtivo) {
          setCarregando(false)
        }
      })

    return () => {
      componenteAtivo = false
    }
  }, [])

  if (carregando) {
    return (
      <main className="pagina-conteudo">
        <p>Carregando técnicos...</p>
      </main>
    )
  }

  if (erro) {
    return (
      <main className="pagina-conteudo">
        <p className="mensagem erro">{erro}</p>
      </main>
    )
  }

  return (
    <main className="pagina-tecnicos">
      <header className="cabecalho-pagina">
        <div>
          <span className="subtitulo">
            Assistência técnica confiável
          </span>

          <h1>
            Encontre o técnico certo para seu equipamento
          </h1>

          <p>
            Consulte profissionais, especialidades e
            disponibilidade.
          </p>
        </div>
      </header>

      <section className="grade-tecnicos">
        {tecnicos.length === 0 && (
          <p>Nenhum técnico cadastrado.</p>
        )}

        {tecnicos.map((tecnico) => (
          <article
            className="cartao-tecnico"
            key={tecnico.id}
          >
            <div className="avatar">
              {tecnico.nomeExibicao
                .charAt(0)
                .toUpperCase()}
            </div>

            <div className="dados-tecnico">
              <div className="titulo-cartao">
                <h2>{tecnico.nomeExibicao}</h2>

                <span
                  className={
                    tecnico.disponivel
                      ? 'status disponivel'
                      : 'status indisponivel'
                  }
                >
                  {tecnico.disponivel
                    ? 'Disponível'
                    : 'Indisponível'}
                </span>
              </div>

              <p>{tecnico.descricaoProfissional}</p>

              <p className="localizacao">
                {tecnico.cidadeAtendimento} -{' '}
                {tecnico.estadoAtendimento}
              </p>

              <div className="especialidades">
                {tecnico.especialidades.length === 0 && (
                  <span>
                    Sem especialidades cadastradas
                  </span>
                )}

                {tecnico.especialidades.map(
                  (especialidade) => (
                    <span
                      className="etiqueta"
                      key={especialidade.especialidadeId}
                    >
                      {especialidade.nome} ·{' '}
                      {especialidade.nivelExperiencia}
                    </span>
                  )
                )}
              </div>
            </div>
          </article>
        ))}
      </section>
    </main>
  )
}