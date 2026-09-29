import { useState } from 'react'
import type { FormEvent } from 'react'
import {
  atualizarExperienciaTecnico,
  desvincularEspecialidadeTecnico
} from '../services/tecnicosApi'
import type { EspecialidadeTecnico } from '../types/Tecnico'

interface EditarExperienciaProps {
  tecnicoId: number
  especialidade: EspecialidadeTecnico
  aoConcluir: () => Promise<void>
}

const niveis: Record<string, number> = {
  Iniciante: 0,
  Intermediario: 1,
  Avancado: 2,
  Especialista: 3
}

export function EditarExperiencia({
  tecnicoId,
  especialidade,
  aoConcluir
}: EditarExperienciaProps) {
  const [editando, setEditando] = useState(false)
  const [salvando, setSalvando] = useState(false)
  const [removendo, setRemovendo] = useState(false)
  const [erro, setErro] = useState('')

  const [nivelExperiencia, setNivelExperiencia] =
    useState(
      niveis[especialidade.nivelExperiencia] ?? 0
    )

  const [anosExperiencia, setAnosExperiencia] =
    useState(especialidade.anosExperiencia)

  async function salvar(
    evento: FormEvent<HTMLFormElement>
  ) {
    evento.preventDefault()
    setSalvando(true)
    setErro('')

    try {
      await atualizarExperienciaTecnico(
        tecnicoId,
        especialidade.especialidadeId,
        {
          nivelExperiencia,
          anosExperiencia
        }
      )

      await aoConcluir()
      setEditando(false)
    } catch (erroRecebido) {
      if (erroRecebido instanceof Error) {
        setErro(erroRecebido.message)
      } else {
        setErro(
          'Não foi possível atualizar a experiência.'
        )
      }
    } finally {
      setSalvando(false)
    }
  }

  async function removerEspecialidade() {
    const confirmou = window.confirm(
      `Deseja remover a especialidade "${especialidade.nome}" do perfil?`
    )

    if (!confirmou) {
      return
    }

    setRemovendo(true)
    setErro('')

    try {
      await desvincularEspecialidadeTecnico(
        tecnicoId,
        especialidade.especialidadeId
      )

      await aoConcluir()
    } catch (erroRecebido) {
      if (erroRecebido instanceof Error) {
        setErro(erroRecebido.message)
      } else {
        setErro(
          'Não foi possível remover a especialidade.'
        )
      }
    } finally {
      setRemovendo(false)
    }
  }

  return (
    <div className="item-experiencia">
      <div className="resumo-experiencia">
        <span className="etiqueta">
          {especialidade.nome} ·{' '}
          {especialidade.nivelExperiencia} ·{' '}
          {especialidade.anosExperiencia} anos
        </span>

        <div className="acoes-item">
          <button
            className="botao-item"
            type="button"
            onClick={() => {
              setEditando(!editando)
              setErro('')
            }}
          >
            {editando ? 'Fechar' : 'Editar experiência'}
          </button>

          <button
            className="botao-item perigo"
            type="button"
            disabled={removendo}
            onClick={removerEspecialidade}
          >
            {removendo ? 'Removendo...' : 'Remover'}
          </button>
        </div>
      </div>

      {editando && (
        <form
          className="formulario-experiencia"
          onSubmit={salvar}
        >
          <label>
            Nível
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

          <button
            className="botao-principal"
            type="submit"
            disabled={salvando}
          >
            {salvando ? 'Salvando...' : 'Salvar'}
          </button>
        </form>
      )}

      {erro && (
        <p className="mensagem erro">{erro}</p>
      )}
    </div>
  )
}