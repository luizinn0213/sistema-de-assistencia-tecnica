import { useEffect, useState } from 'react'
import { listarTecnicos } from '../services/tecnicosApi'
import type { Tecnico } from '../types/Tecnico'

export function ListaTecnicos() {
  const [tecnicos, setTecnicos] = useState<Tecnico[]>([])
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')

  useEffect(() => {
    async function carregarTecnicos() {
      try {
        const dados = await listarTecnicos()
        setTecnicos(dados)
      } catch {
        setErro('Não foi possível carregar os técnicos.')
      } finally {
        setCarregando(false)
      }
    }

    carregarTecnicos()
  }, [])

  if (carregando) {
    return <p>Carregando técnicos...</p>
  }

  if (erro) {
    return <p>{erro}</p>
  }

  return (
    <main className="pagina-tecnicos">
      <header className="cabecalho-pagina">
        <div>
          <span className="subtitulo">Profissionais cadastrados</span>
          <h1>Encontre assistência técnica</h1>
          <p>
            Consulte profissionais, especialidades e disponibilidade.
          </p>
        </div>
      </header>

      <section className="grade-tecnicos">
        {tecnicos.length === 0 && (
          <p>Nenhum técnico cadastrado.</p>
        )}

        {tecnicos.map((tecnico) => (
          <article className="cartao-tecnico" key={tecnico.id}>
            <div className="avatar">
              {tecnico.nomeExibicao.charAt(0).toUpperCase()}
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
                {tecnico.cidadeAtendimento} - {tecnico.estadoAtendimento}
              </p>

              <div className="especialidades">
                {tecnico.especialidades.length === 0 && (
                  <span>Sem especialidades cadastradas</span>
                )}

                {tecnico.especialidades.map((especialidade) => (
                  <span
                    className="etiqueta"
                    key={especialidade.especialidadeId}
                  >
                    {especialidade.nome} ·{' '}
                    {especialidade.nivelExperiencia}
                  </span>
                ))}
              </div>
            </div>
          </article>
        ))}
      </section>
    </main>
  )
}