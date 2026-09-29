import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { listarEspecialidades } from '../services/especialidadesApi'
import { adicionarEspecialidadeTecnico } from '../services/tecnicosApi'
import type { Especialidade } from '../types/Especialidade'

interface VincularEspecialidadeProps {
  tecnicoId: number
  especialidadesAtuais: number[]
  aoConcluir: () => Promise<void>
  aoCancelar: () => void
}

export function VincularEspecialidade({
  tecnicoId,
  especialidadesAtuais,
  aoConcluir,
  aoCancelar
}: VincularEspecialidadeProps) {
  const [especialidades, setEspecialidades] =
    useState<Especialidade[]>([])

  const [especialidadeId, setEspecialidadeId] =
    useState(0)

  const [nivelExperiencia, setNivelExperiencia] =
    useState(0)

  const [anosExperiencia, setAnosExperiencia] =
    useState(0)

  const [enviando, setEnviando] = useState(false)
  const [erro, setErro] = useState('')

  useEffect(() => {
  let componenteAtivo = true

    listarEspecialidades()
        .then((dados) => {
        if (componenteAtivo) {
            setEspecialidades(dados)
        }
        })
        .catch(() => {
        if (componenteAtivo) {
            setErro(
            'Não foi possível carregar as especialidades.'
            )
        }
        })

    return () => {
        componenteAtivo = false
    }
  }, [])

  const especialidadesDisponiveis =
    especialidades.filter(
      (especialidade) =>
        especialidade.ativa &&
        !especialidadesAtuais.includes(especialidade.id)
    )

  async function enviar(
    evento: FormEvent<HTMLFormElement>
  ) {
    evento.preventDefault()
    setEnviando(true)
    setErro('')

    try {
      await adicionarEspecialidadeTecnico(
        tecnicoId,
        {
          especialidadeId,
          nivelExperiencia,
          anosExperiencia
        }
      )

      await aoConcluir()
      aoCancelar()
    } catch (erroRecebido) {
      if (erroRecebido instanceof Error) {
        setErro(erroRecebido.message)
      } else {
        setErro('Não foi possível vincular a especialidade.')
      }
    } finally {
      setEnviando(false)
    }
  }

  if (especialidadesDisponiveis.length === 0) {
    return (
      <div className="formulario-vinculo">
        <p>
          Não existem outras especialidades ativas disponíveis.
        </p>

        <button
          className="botao-secundario"
          type="button"
          onClick={aoCancelar}
        >
          Fechar
        </button>
      </div>
    )
  }

  return (
    <form
      className="formulario-vinculo"
      onSubmit={enviar}
    >
      <h3>Adicionar especialidade</h3>

      <label>
        Especialidade
        <select
          required
          value={especialidadeId || ''}
          onChange={(evento) =>
            setEspecialidadeId(
              Number(evento.target.value)
            )
          }
        >
          <option value="">
            Selecione uma especialidade
          </option>

          {especialidadesDisponiveis.map(
            (especialidade) => (
              <option
                key={especialidade.id}
                value={especialidade.id}
              >
                {especialidade.nome}
              </option>
            )
          )}
        </select>
      </label>

      <label>
        Nível de experiência
        <select
          value={nivelExperiencia}
          onChange={(evento) =>
            setNivelExperiencia(
              Number(evento.target.value)
            )
          }
        >
          <option value={0}>Iniciante</option>
          <option value={1}>Intermediário</option>
          <option value={2}>Avançado</option>
          <option value={3}>Especialista</option>
        </select>
      </label>

      <label>
        Anos de experiência
        <input
          type="number"
          min="0"
          max="80"
          required
          value={anosExperiencia}
          onChange={(evento) =>
            setAnosExperiencia(
              Number(evento.target.value)
            )
          }
        />
      </label>

      {erro && (
        <p className="mensagem erro">{erro}</p>
      )}

      <div className="acoes-formulario">
        <button
          className="botao-secundario"
          type="button"
          onClick={aoCancelar}
        >
          Cancelar
        </button>

        <button
          className="botao-principal"
          type="submit"
          disabled={enviando}
        >
          {enviando ? 'Salvando...' : 'Adicionar'}
        </button>
      </div>
    </form>
  )
}