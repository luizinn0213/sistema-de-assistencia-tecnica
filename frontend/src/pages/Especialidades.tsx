import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import {
  cadastrarEspecialidade,
  listarEspecialidades
} from '../services/especialidadesApi'
import type { Especialidade } from '../types/Especialidade'

export function Especialidades() {
  const [especialidades, setEspecialidades] =
    useState<Especialidade[]>([])

  const [nome, setNome] = useState('')
  const [descricao, setDescricao] = useState('')
  const [carregando, setCarregando] = useState(true)
  const [enviando, setEnviando] = useState(false)
  const [mensagem, setMensagem] = useState('')
  const [erro, setErro] = useState('')

  useEffect(() => {
    async function carregar() {
      try {
        const dados = await listarEspecialidades()
        setEspecialidades(dados)
      } catch {
        setErro('Não foi possível carregar as especialidades.')
      } finally {
        setCarregando(false)
      }
    }

    carregar()
  }, [])

  async function enviarFormulario(
    evento: FormEvent<HTMLFormElement>
  ) {
    evento.preventDefault()

    setEnviando(true)
    setMensagem('')
    setErro('')

    try {
      const novaEspecialidade =
        await cadastrarEspecialidade({
          nome,
          descricao
        })

      setEspecialidades([
        ...especialidades,
        novaEspecialidade
      ])

      setNome('')
      setDescricao('')
      setMensagem('Especialidade cadastrada com sucesso.')
    } catch (erroRecebido) {
      if (erroRecebido instanceof Error) {
        setErro(erroRecebido.message)
      } else {
        setErro(
          'Não foi possível cadastrar a especialidade.'
        )
      }
    } finally {
      setEnviando(false)
    }
  }

  return (
    <main className="pagina-especialidades">
      <div className="cabecalho-formulario">
        <span className="subtitulo-formulario">
          Áreas de atendimento
        </span>

        <h1>Especialidades</h1>

        <p>
          Cadastre e consulte as especialidades da plataforma.
        </p>
      </div>

      <div className="layout-especialidades">
        <form
          className="formulario painel-formulario"
          onSubmit={enviarFormulario}
        >
          <h2>Nova especialidade</h2>

          <label>
            Nome
            <input
              type="text"
              required
              value={nome}
              onChange={(evento) =>
                setNome(evento.target.value)
              }
            />
          </label>

          <label>
            Descrição
            <textarea
              rows={4}
              required
              value={descricao}
              onChange={(evento) =>
                setDescricao(evento.target.value)
              }
            />
          </label>

          {mensagem && (
            <p className="mensagem sucesso">
              {mensagem}
            </p>
          )}

          {erro && (
            <p className="mensagem erro">{erro}</p>
          )}

          <button
            className="botao-principal"
            type="submit"
            disabled={enviando}
          >
            {enviando
              ? 'Cadastrando...'
              : 'Cadastrar especialidade'}
          </button>
        </form>

        <section className="lista-especialidades">
          <h2>Especialidades cadastradas</h2>

          {carregando && <p>Carregando...</p>}

          {!carregando &&
            especialidades.length === 0 && (
              <p>Nenhuma especialidade cadastrada.</p>
            )}

          {especialidades.map((especialidade) => (
            <article
              className="item-especialidade"
              key={especialidade.id}
            >
              <div>
                <h3>{especialidade.nome}</h3>
                <p>{especialidade.descricao}</p>
              </div>

              <span
                className={
                  especialidade.ativa
                    ? 'status-item ativo'
                    : 'status-item inativo'
                }
              >
                {especialidade.ativa
                  ? 'Ativa'
                  : 'Inativa'}
              </span>
            </article>
          ))}
        </section>
      </div>
    </main>
  )
}