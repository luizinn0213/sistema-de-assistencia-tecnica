import { useEffect, useState } from 'react'
import { VincularEspecialidade } from '../components/VincularEspecialidade'
import {
  alterarDisponibilidadeTecnico,
  listarTecnicos
} from '../services/tecnicosApi'
import type { Tecnico } from '../types/Tecnico'

export function PerfilTecnico() {
  const [tecnicos, setTecnicos] = useState<Tecnico[]>([])
  const [tecnicoId, setTecnicoId] = useState(0)
  const [mostrarVinculo, setMostrarVinculo] = useState(false)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [erroAcao, setErroAcao] = useState('')
  const [
    alterandoDisponibilidade,
    setAlterandoDisponibilidade
  ] = useState(false)

  useEffect(() => {
    let componenteAtivo = true

    listarTecnicos()
      .then((dados) => {
        if (componenteAtivo) {
          setTecnicos(dados)

          if (dados.length > 0) {
            setTecnicoId(dados[0].id)
          }
        }
      })
      .catch(() => {
        if (componenteAtivo) {
          setErro('Não foi possível carregar o perfil.')
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

  async function atualizarTecnicos() {
    const dados = await listarTecnicos()
    setTecnicos(dados)
  }

  const tecnico = tecnicos.find(
    (item) => item.id === tecnicoId
  )

  async function alternarDisponibilidade() {
    if (!tecnico) {
      return
    }

    setAlterandoDisponibilidade(true)
    setErroAcao('')

    try {
      await alterarDisponibilidadeTecnico(
        tecnico.id,
        !tecnico.disponivel
      )

      await atualizarTecnicos()
    } catch (erroRecebido) {
      if (erroRecebido instanceof Error) {
        setErroAcao(erroRecebido.message)
      } else {
        setErroAcao(
          'Não foi possível alterar a disponibilidade.'
        )
      }
    } finally {
      setAlterandoDisponibilidade(false)
    }
  }

  if (carregando) {
    return (
      <main className="pagina-conteudo">
        <p>Carregando perfil...</p>
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

  if (!tecnico) {
    return (
      <main className="pagina-conteudo">
        <h1>Meu perfil técnico</h1>
        <p>Nenhum perfil técnico cadastrado.</p>
      </main>
    )
  }

  let textoDisponibilidade = 'Ficar disponível'

  if (alterandoDisponibilidade) {
    textoDisponibilidade = 'Alterando...'
  } else if (tecnico.disponivel) {
    textoDisponibilidade = 'Ficar indisponível'
  }

  return (
    <main className="pagina-perfil-tecnico">
      <div className="cabecalho-formulario">
        <span className="subtitulo-formulario">
          Área profissional
        </span>

        <h1>Meu perfil técnico</h1>

        <p>
          Gerencie suas especialidades e disponibilidade.
        </p>
      </div>

      <label className="seletor-demonstracao">
        Técnico utilizado na demonstração

        <select
          value={tecnicoId}
          onChange={(evento) => {
            setTecnicoId(Number(evento.target.value))
            setMostrarVinculo(false)
            setErroAcao('')
          }}
        >
          {tecnicos.map((item) => (
            <option key={item.id} value={item.id}>
              {item.nomeExibicao}
            </option>
          ))}
        </select>

        <small>
          Esse seletor será removido quando o login estiver
          integrado.
        </small>
      </label>

      <section className="painel-perfil-tecnico">
        <div className="avatar avatar-perfil">
          {tecnico.nomeExibicao.charAt(0).toUpperCase()}
        </div>

        <div className="informacoes-perfil">
          <div className="titulo-perfil">
            <div>
              <h2>{tecnico.nomeExibicao}</h2>
              <p>{tecnico.descricaoProfissional}</p>
            </div>

            <span
              className={
                tecnico.disponivel
                  ? 'status-perfil disponivel'
                  : 'status-perfil indisponivel'
              }
            >
              {tecnico.disponivel
                ? 'Disponível'
                : 'Indisponível'}
            </span>
          </div>

          <p className="localizacao">
            {tecnico.cidadeAtendimento} -{' '}
            {tecnico.estadoAtendimento}
          </p>

          <h3>Especialidades</h3>

          <div className="especialidades">
            {tecnico.especialidades.length === 0 && (
              <span>Nenhuma especialidade vinculada.</span>
            )}

            {tecnico.especialidades.map(
              (especialidade) => (
                <span
                  className="etiqueta"
                  key={especialidade.especialidadeId}
                >
                  {especialidade.nome} ·{' '}
                  {especialidade.nivelExperiencia} ·{' '}
                  {especialidade.anosExperiencia} anos
                </span>
              )
            )}
          </div>

          <div className="acoes-perfil">
            <button
              className="botao-principal"
              type="button"
              onClick={() =>
                setMostrarVinculo(!mostrarVinculo)
              }
            >
              Adicionar especialidade
            </button>

            <button
              className="botao-secundario"
              type="button"
              disabled={alterandoDisponibilidade}
              onClick={alternarDisponibilidade}
            >
              {textoDisponibilidade}
            </button>
          </div>

          {erroAcao && (
            <p className="mensagem erro">{erroAcao}</p>
          )}

          {mostrarVinculo && (
            <VincularEspecialidade
              tecnicoId={tecnico.id}
              especialidadesAtuais={tecnico.especialidades.map(
                (especialidade) =>
                  especialidade.especialidadeId
              )}
              aoConcluir={atualizarTecnicos}
              aoCancelar={() => setMostrarVinculo(false)}
            />
          )}
        </div>
      </section>
    </main>
  )
}