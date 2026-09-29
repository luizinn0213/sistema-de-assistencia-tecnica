import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import {
  alterarStatusEspecialidade,
  atualizarEspecialidade,
  cadastrarEspecialidade,
  listarEspecialidades
} from '../services/especialidadesApi'
import type { Especialidade } from '../types/Especialidade'

function ordenarEspecialidades(
  especialidades: Especialidade[]
) {
  return [...especialidades].sort((a, b) =>
    a.nome.localeCompare(b.nome, 'pt-BR')
  )
}

export function Especialidades() {
  const [especialidades, setEspecialidades] =
    useState<Especialidade[]>([])

  const [nome, setNome] = useState('')
  const [descricao, setDescricao] = useState('')
  const [editandoId, setEditandoId] =
    useState<number | null>(null)

  const [carregando, setCarregando] = useState(true)
  const [enviando, setEnviando] = useState(false)
  const [alterandoStatusId, setAlterandoStatusId] =
    useState<number | null>(null)

  const [mensagem, setMensagem] = useState('')
  const [erro, setErro] = useState('')

  useEffect(() => {
    let componenteAtivo = true

    listarEspecialidades()
      .then((dados) => {
        if (componenteAtivo) {
          setEspecialidades(
            ordenarEspecialidades(dados)
          )
        }
      })
      .catch(() => {
        if (componenteAtivo) {
          setErro(
            'Não foi possível carregar as especialidades.'
          )
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

  function limparFormulario() {
    setNome('')
    setDescricao('')
    setEditandoId(null)
  }

  function iniciarEdicao(
    especialidade: Especialidade
  ) {
    setEditandoId(especialidade.id)
    setNome(especialidade.nome)
    setDescricao(especialidade.descricao)
    setMensagem('')
    setErro('')
  }

  function cancelarEdicao() {
    limparFormulario()
    setMensagem('')
    setErro('')
  }

  async function enviarFormulario(
    evento: FormEvent<HTMLFormElement>
  ) {
    evento.preventDefault()

    setEnviando(true)
    setMensagem('')
    setErro('')

    try {
      if (editandoId !== null) {
        const especialidadeAtualizada =
          await atualizarEspecialidade(
            editandoId,
            { nome, descricao }
          )

        setEspecialidades((atuais) =>
          ordenarEspecialidades(
            atuais.map((especialidade) =>
              especialidade.id === editandoId
                ? especialidadeAtualizada
                : especialidade
            )
          )
        )

        setMensagem(
          'Especialidade atualizada com sucesso.'
        )
      } else {
        const novaEspecialidade =
          await cadastrarEspecialidade({
            nome,
            descricao
          })

        setEspecialidades((atuais) =>
          ordenarEspecialidades([
            ...atuais,
            novaEspecialidade
          ])
        )

        setMensagem(
          'Especialidade cadastrada com sucesso.'
        )
      }

      limparFormulario()
    } catch (erroRecebido) {
      if (erroRecebido instanceof Error) {
        setErro(erroRecebido.message)
      } else {
        setErro(
          'Não foi possível salvar a especialidade.'
        )
      }
    } finally {
      setEnviando(false)
    }
  }

  async function alternarStatus(
    especialidade: Especialidade
  ) {
    setAlterandoStatusId(especialidade.id)
    setMensagem('')
    setErro('')

    try {
      const especialidadeAtualizada =
        await alterarStatusEspecialidade(
          especialidade.id,
          !especialidade.ativa
        )

      setEspecialidades((atuais) =>
        atuais.map((item) =>
          item.id === especialidade.id
            ? especialidadeAtualizada
            : item
        )
      )

      setMensagem(
        especialidadeAtualizada.ativa
          ? 'Especialidade ativada com sucesso.'
          : 'Especialidade inativada com sucesso.'
      )
    } catch (erroRecebido) {
      if (erroRecebido instanceof Error) {
        setErro(erroRecebido.message)
      } else {
        setErro(
          'Não foi possível alterar o status.'
        )
      }
    } finally {
      setAlterandoStatusId(null)
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
          Cadastre e gerencie as especialidades da
          plataforma.
        </p>
      </div>

      <div className="layout-especialidades">
        <form
          className="formulario painel-formulario"
          onSubmit={enviarFormulario}
        >
          <h2>
            {editandoId === null
              ? 'Nova especialidade'
              : 'Editar especialidade'}
          </h2>

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

          <div className="acoes-formulario">
            <button
              className="botao-principal"
              type="submit"
              disabled={enviando}
            >
              {enviando
                ? 'Salvando...'
                : editandoId === null
                  ? 'Cadastrar especialidade'
                  : 'Salvar alterações'}
            </button>

            {editandoId !== null && (
              <button
                className="botao-secundario"
                type="button"
                onClick={cancelarEdicao}
              >
                Cancelar
              </button>
            )}
          </div>
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
              <div className="dados-especialidade">
                <h3>{especialidade.nome}</h3>
                <p>{especialidade.descricao}</p>
              </div>

              <div className="controle-especialidade">
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

                <div className="acoes-item">
                  <button
                    className="botao-item"
                    type="button"
                    onClick={() =>
                      iniciarEdicao(especialidade)
                    }
                  >
                    Editar
                  </button>

                  <button
                    className={
                      especialidade.ativa
                        ? 'botao-item perigo'
                        : 'botao-item ativar'
                    }
                    type="button"
                    disabled={
                      alterandoStatusId ===
                      especialidade.id
                    }
                    onClick={() =>
                      alternarStatus(especialidade)
                    }
                  >
                    {alterandoStatusId ===
                    especialidade.id
                      ? 'Alterando...'
                      : especialidade.ativa
                        ? 'Inativar'
                        : 'Ativar'}
                  </button>
                </div>
              </div>
            </article>
          ))}
        </section>
      </div>
    </main>
  )
}