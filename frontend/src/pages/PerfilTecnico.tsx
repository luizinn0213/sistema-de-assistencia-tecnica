import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { EditarExperiencia } from '../components/EditarExperiencia'
import { Toast } from '../components/Toast'
import { VincularEspecialidade } from '../components/VincularEspecialidade'
import {
  alterarDisponibilidadeTecnico,
  alterarStatusTecnico,
  atualizarTecnico,
  listarTecnicos
} from '../services/tecnicosApi'
import type { Tecnico } from '../types/Tecnico'

export function PerfilTecnico() {
  const [tecnicos, setTecnicos] = useState<Tecnico[]>([])
  const [tecnicoId, setTecnicoId] = useState(0)

  const [mostrarVinculo, setMostrarVinculo] =
    useState(false)

  const [editandoDados, setEditandoDados] =
    useState(false)

  const [nomeExibicao, setNomeExibicao] = useState('')

  const [
    descricaoProfissional,
    setDescricaoProfissional
  ] = useState('')

  const [
    cidadeAtendimento,
    setCidadeAtendimento
  ] = useState('')

  const [
    estadoAtendimento,
    setEstadoAtendimento
  ] = useState('')

  const [carregando, setCarregando] = useState(true)

  const [salvandoDados, setSalvandoDados] =
    useState(false)

  const [
    alterandoDisponibilidade,
    setAlterandoDisponibilidade
  ] = useState(false)

  const [alterandoStatus, setAlterandoStatus] =
    useState(false)

  const [erro, setErro] = useState('')
  const [erroAcao, setErroAcao] = useState('')
  const [mensagemAcao, setMensagemAcao] = useState('')

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

  function iniciarEdicaoDados() {
    if (!tecnico) {
      return
    }

    setNomeExibicao(tecnico.nomeExibicao)

    setDescricaoProfissional(
      tecnico.descricaoProfissional
    )

    setCidadeAtendimento(
      tecnico.cidadeAtendimento
    )

    setEstadoAtendimento(
      tecnico.estadoAtendimento
    )

    setEditandoDados(true)
    setErroAcao('')
    setMensagemAcao('')
  }

  async function salvarDados(
    evento: FormEvent<HTMLFormElement>
  ) {
    evento.preventDefault()

    if (!tecnico) {
      return
    }

    setSalvandoDados(true)
    setErroAcao('')
    setMensagemAcao('')

    try {
      await atualizarTecnico(tecnico.id, {
        nomeExibicao,
        descricaoProfissional,
        cidadeAtendimento,
        estadoAtendimento
      })

      await atualizarTecnicos()

      setEditandoDados(false)

      setMensagemAcao(
        'Dados profissionais atualizados.'
      )
    } catch (erroRecebido) {
      if (erroRecebido instanceof Error) {
        setErroAcao(erroRecebido.message)
      } else {
        setErroAcao(
          'Não foi possível atualizar os dados.'
        )
      }
    } finally {
      setSalvandoDados(false)
    }
  }

  async function alternarDisponibilidade() {
    if (!tecnico) {
      return
    }

    setAlterandoDisponibilidade(true)
    setErroAcao('')
    setMensagemAcao('')

    try {
      await alterarDisponibilidadeTecnico(
        tecnico.id,
        !tecnico.disponivel
      )

      await atualizarTecnicos()

      setMensagemAcao(
        'Disponibilidade atualizada.'
      )
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

  async function alternarStatus() {
    if (!tecnico) {
      return
    }

    setAlterandoStatus(true)
    setErroAcao('')
    setMensagemAcao('')

    try {
      await alterarStatusTecnico(
        tecnico.id,
        !tecnico.ativo
      )

      await atualizarTecnicos()

      setMensagemAcao(
        tecnico.ativo
          ? 'Técnico inativado.'
          : 'Técnico ativado.'
      )
    } catch (erroRecebido) {
      if (erroRecebido instanceof Error) {
        setErroAcao(erroRecebido.message)
      } else {
        setErroAcao(
          'Não foi possível alterar o status.'
        )
      }
    } finally {
      setAlterandoStatus(false)
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
  } else if (!tecnico.ativo) {
    textoDisponibilidade = 'Técnico inativo'
  } else if (tecnico.disponivel) {
    textoDisponibilidade = 'Ficar indisponível'
  }

  let textoStatus = 'Inativar técnico'

  if (alterandoStatus) {
    textoStatus = 'Alterando...'
  } else if (!tecnico.ativo) {
    textoStatus = 'Ativar técnico'
  }

  return (
    <>
      {mensagemAcao && (
        <Toast
          mensagem={mensagemAcao}
          tipo="sucesso"
          aoFechar={() => setMensagemAcao('')}
        />
      )}

      {erroAcao && (
        <Toast
          mensagem={erroAcao}
          tipo="erro"
          aoFechar={() => setErroAcao('')}
        />
      )}

      <main className="pagina-perfil-tecnico">
        <div className="cabecalho-formulario">
          <span className="subtitulo-formulario">
            Área profissional
          </span>

          <h1>Meu perfil técnico</h1>

          <p>
            Gerencie seus dados, especialidades e
            disponibilidade.
          </p>
        </div>

        <label className="seletor-demonstracao">
          Técnico utilizado na demonstração

          <select
            value={tecnicoId}
            onChange={(evento) => {
              setTecnicoId(Number(evento.target.value))
              setMostrarVinculo(false)
              setEditandoDados(false)
              setErroAcao('')
              setMensagemAcao('')
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
            {tecnico.nomeExibicao
              .charAt(0)
              .toUpperCase()}
          </div>

          <div className="informacoes-perfil">
            <div className="titulo-perfil">
              <div>
                <h2>{tecnico.nomeExibicao}</h2>
                <p>
                  {tecnico.descricaoProfissional}
                </p>
              </div>

              <div className="grupo-status-perfil">
                <span
                  className={
                    tecnico.ativo
                      ? 'status-perfil ativo'
                      : 'status-perfil inativo'
                  }
                >
                  {tecnico.ativo ? 'Ativo' : 'Inativo'}
                </span>

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
            </div>

            <p className="localizacao">
              {tecnico.cidadeAtendimento} -{' '}
              {tecnico.estadoAtendimento}
            </p>

            {editandoDados && (
              <form
                className="formulario-edicao-tecnico"
                onSubmit={salvarDados}
              >
                <label>
                  Nome de exibição
                  <input
                    type="text"
                    required
                    value={nomeExibicao}
                    onChange={(evento) =>
                      setNomeExibicao(
                        evento.target.value
                      )
                    }
                  />
                </label>

                <label>
                  Descrição profissional
                  <textarea
                    rows={3}
                    value={descricaoProfissional}
                    onChange={(evento) =>
                      setDescricaoProfissional(
                        evento.target.value
                      )
                    }
                  />
                </label>

                <div className="linha-formulario">
                  <label>
                    Cidade
                    <input
                      type="text"
                      required
                      value={cidadeAtendimento}
                      onChange={(evento) =>
                        setCidadeAtendimento(
                          evento.target.value
                        )
                      }
                    />
                  </label>

                  <label>
                    Estado
                    <input
                      type="text"
                      required
                      maxLength={2}
                      value={estadoAtendimento}
                      onChange={(evento) =>
                        setEstadoAtendimento(
                          evento.target.value
                            .toUpperCase()
                        )
                      }
                    />
                  </label>
                </div>

                <div className="acoes-formulario">
                  <button
                    className="botao-principal"
                    type="submit"
                    disabled={salvandoDados}
                  >
                    {salvandoDados
                      ? 'Salvando...'
                      : 'Salvar dados'}
                  </button>

                  <button
                    className="botao-secundario"
                    type="button"
                    onClick={() =>
                      setEditandoDados(false)
                    }
                  >
                    Cancelar
                  </button>
                </div>
              </form>
            )}

            <h3>Especialidades</h3>

            <div className="lista-experiencias">
              {tecnico.especialidades.length === 0 && (
                <span>
                  Nenhuma especialidade vinculada.
                </span>
              )}

              {tecnico.especialidades.map(
                (especialidade) => (
                  <EditarExperiencia
                    key={`${tecnico.id}-${especialidade.especialidadeId}`}
                    tecnicoId={tecnico.id}
                    especialidade={especialidade}
                    aoConcluir={atualizarTecnicos}
                  />
                )
              )}
            </div>

            <div className="acoes-perfil">
              <button
                className="botao-secundario"
                type="button"
                onClick={iniciarEdicaoDados}
              >
                Editar dados
              </button>

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
                disabled={
                  alterandoDisponibilidade ||
                  !tecnico.ativo
                }
                onClick={alternarDisponibilidade}
              >
                {textoDisponibilidade}
              </button>

              <button
                className={
                  tecnico.ativo
                    ? 'botao-item perigo'
                    : 'botao-item ativar'
                }
                type="button"
                disabled={alterandoStatus}
                onClick={alternarStatus}
              >
                {textoStatus}
              </button>
            </div>

            {mostrarVinculo && (
              <VincularEspecialidade
                tecnicoId={tecnico.id}
                especialidadesAtuais={
                  tecnico.especialidades.map(
                    (especialidade) =>
                      especialidade.especialidadeId
                  )
                }
                aoConcluir={atualizarTecnicos}
                aoCancelar={() =>
                  setMostrarVinculo(false)
                }
              />
            )}
          </div>
        </section>
      </main>
    </>
  )
}