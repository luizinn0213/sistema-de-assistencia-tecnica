import { useEffect, useState } from 'react'
import { Toast } from '../components/Toast'
import {
    aceitarSelecao,
    listarHistoricoSelecao,
    listarSelecoesPendentes,
    recusarSelecao
} from '../services/matchingApi'
import { listarTecnicos } from '../services/tecnicosApi'
import type {
    HistoricoSelecao,
    SelecaoTecnico
} from '../types/Matching'
import type { Tecnico } from '../types/Tecnico'

export function SolicitacoesTecnico() {
    const [tecnicos, setTecnicos] = useState<Tecnico[]>([])
    const [tecnicoId, setTecnicoId] = useState(0)
    const [selecoes, setSelecoes] =
        useState<SelecaoTecnico[]>([])
    const [historico, setHistorico] =
        useState<HistoricoSelecao[]>([])

    const [selecaoRecusadaId, setSelecaoRecusadaId] =
        useState<number | null>(null)
    const [motivo, setMotivo] = useState('')
    const [carregando, setCarregando] = useState(true)
    const [processandoId, setProcessandoId] =
        useState<number | null>(null)

    const [toast, setToast] = useState<{
        mensagem: string
        tipo: 'sucesso' | 'erro'
    } | null>(null)

    useEffect(() => {
        listarTecnicos()
            .then((dados) => {
                setTecnicos(dados)

                if (dados.length > 0) {
                    setTecnicoId(dados[0].id)
                }
            })
            .catch(() => {
                setToast({
                    mensagem:
                        'Não foi possível carregar os técnicos.',
                    tipo: 'erro'
                })
            })
            .finally(() => {
                setCarregando(false)
            })
    }, [])

    useEffect(() => {
        if (tecnicoId > 0) {
            carregarPendentes(tecnicoId)
        }
    }, [tecnicoId])

    async function carregarPendentes(id: number) {
        try {
            const dados = await listarSelecoesPendentes(id)
            setSelecoes(dados)
        } catch (erroRecebido) {
            const mensagem =
                erroRecebido instanceof Error
                    ? erroRecebido.message
                    : 'Não foi possível carregar as solicitações.'

            setToast({
                mensagem,
                tipo: 'erro'
            })
        }
    }

    async function mostrarHistorico(selecaoId: number) {
        try {
            const dados = await listarHistoricoSelecao(
                selecaoId
            )

            setHistorico(dados)
        } catch (erroRecebido) {
            const mensagem =
                erroRecebido instanceof Error
                    ? erroRecebido.message
                    : 'Não foi possível carregar o histórico.'

            setToast({
                mensagem,
                tipo: 'erro'
            })
        }
    }

    async function aceitar(selecaoId: number) {
        setProcessandoId(selecaoId)

        try {
            await aceitarSelecao(selecaoId)
            await carregarPendentes(tecnicoId)
            await mostrarHistorico(selecaoId)

            setToast({
                mensagem: 'Solicitação aceita com sucesso.',
                tipo: 'sucesso'
            })
        } catch (erroRecebido) {
            const mensagem =
                erroRecebido instanceof Error
                    ? erroRecebido.message
                    : 'Não foi possível aceitar a solicitação.'

            setToast({
                mensagem,
                tipo: 'erro'
            })
        } finally {
            setProcessandoId(null)
        }
    }

    async function recusar(selecaoId: number) {
        setProcessandoId(selecaoId)

        try {
            await recusarSelecao(selecaoId, motivo)
            await carregarPendentes(tecnicoId)
            await mostrarHistorico(selecaoId)

            setSelecaoRecusadaId(null)
            setMotivo('')

            setToast({
                mensagem: 'Solicitação recusada.',
                tipo: 'sucesso'
            })
        } catch (erroRecebido) {
            const mensagem =
                erroRecebido instanceof Error
                    ? erroRecebido.message
                    : 'Não foi possível recusar a solicitação.'

            setToast({
                mensagem,
                tipo: 'erro'
            })
        } finally {
            setProcessandoId(null)
        }
    }

    if (carregando) {
        return (
            <main className="pagina-conteudo">
                <p>Carregando solicitações...</p>
            </main>
        )
    }

    return (
        <main className="pagina-solicitacoes-tecnico">
            {toast && (
                <Toast
                    mensagem={toast.mensagem}
                    tipo={toast.tipo}
                    aoFechar={() => setToast(null)}
                />
            )}

            <div className="cabecalho-formulario">
                <span className="subtitulo-formulario">
                    Área profissional
                </span>

                <h1>Solicitações recebidas</h1>

                <p>
                    Analise as solicitações encaminhadas e escolha
                    entre aceitar ou recusar o atendimento.
                </p>
            </div>

            <section className="conteudo-solicitacoes">
                <label className="seletor-demonstracao">
                    Técnico utilizado na demonstração

                    <select
                        value={tecnicoId}
                        onChange={(evento) => {
                            setTecnicoId(
                                Number(evento.target.value)
                            )
                            setHistorico([])
                            setSelecaoRecusadaId(null)
                        }}
                    >
                        {tecnicos.map((tecnico) => (
                            <option
                                key={tecnico.id}
                                value={tecnico.id}
                            >
                                {tecnico.nomeExibicao}
                            </option>
                        ))}
                    </select>

                    <small>
                        Esse seletor será removido quando o login
                        do técnico estiver integrado.
                    </small>
                </label>

                {tecnicos.length === 0 && (
                    <p className="estado-vazio">
                        Nenhum técnico cadastrado.
                    </p>
                )}

                {tecnicos.length > 0 &&
                    selecoes.length === 0 && (
                        <p className="estado-vazio">
                            Nenhuma solicitação aguardando resposta.
                        </p>
                    )}

                <div className="lista-solicitacoes">
                    {selecoes.map((selecao) => (
                        <article
                            className="cartao-solicitacao"
                            key={selecao.id}
                        >
                            <div className="topo-solicitacao">
                                <div>
                                    <span>
                                        Solicitação #
                                        {selecao.solicitacaoId}
                                    </span>

                                    <h2>
                                        {selecao.especialidade}
                                    </h2>
                                </div>

                                <strong>
                                    Aguardando resposta
                                </strong>
                            </div>

                            <p>
                                Selecionada em{' '}
                                {new Date(
                                    selecao.dataSelecao
                                ).toLocaleString('pt-BR')}
                            </p>

                            <div className="acoes-solicitacao">
                                <button
                                    className="botao-principal"
                                    type="button"
                                    disabled={
                                        processandoId === selecao.id
                                    }
                                    onClick={() =>
                                        aceitar(selecao.id)
                                    }
                                >
                                    Aceitar
                                </button>

                                <button
                                    className="botao-perigo"
                                    type="button"
                                    disabled={
                                        processandoId === selecao.id
                                    }
                                    onClick={() => {
                                        setSelecaoRecusadaId(
                                            selecao.id
                                        )
                                        setMotivo('')
                                    }}
                                >
                                    Recusar
                                </button>

                                <button
                                    className="botao-secundario"
                                    type="button"
                                    onClick={() =>
                                        mostrarHistorico(
                                            selecao.id
                                        )
                                    }
                                >
                                    Ver histórico
                                </button>
                            </div>

                            {selecaoRecusadaId === selecao.id && (
                                <div className="formulario-recusa">
                                    <label>
                                        Motivo da recusa
                                        <textarea
                                            value={motivo}
                                            onChange={(evento) =>
                                                setMotivo(
                                                    evento.target.value
                                                )
                                            }
                                            placeholder="Informe o motivo, se desejar"
                                        />
                                    </label>

                                    <div>
                                        <button
                                            className="botao-perigo"
                                            type="button"
                                            disabled={
                                                processandoId ===
                                                selecao.id
                                            }
                                            onClick={() =>
                                                recusar(selecao.id)
                                            }
                                        >
                                            Confirmar recusa
                                        </button>

                                        <button
                                            className="botao-secundario"
                                            type="button"
                                            onClick={() => {
                                                setSelecaoRecusadaId(
                                                    null
                                                )
                                                setMotivo('')
                                            }}
                                        >
                                            Cancelar
                                        </button>
                                    </div>
                                </div>
                            )}
                        </article>
                    ))}
                </div>

                {historico.length > 0 && (
                    <section className="painel-historico">
                        <h2>Histórico da seleção</h2>

                        <div className="linha-do-tempo">
                            {historico.map((item) => (
                                <article key={item.id}>
                                    <span
                                        className={`marcador-historico ${item.status.toLowerCase()}`}
                                    />

                                    <div>
                                        <strong>{item.status}</strong>
                                        <p>{item.observacao}</p>
                                        <small>
                                            {new Date(
                                                item.dataRegistro
                                            ).toLocaleString('pt-BR')}
                                        </small>
                                    </div>
                                </article>
                            ))}
                        </div>
                    </section>
                )}
            </section>
        </main>
    )
}