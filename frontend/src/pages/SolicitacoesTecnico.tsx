import { useEffect, useState } from 'react'
import { Toast } from '../components/Toast'
import {
    aceitarSelecao,
    listarHistoricoSelecao,
    listarSelecoesPendentes,
    recusarSelecao
} from '../services/matchingApi'
import { obterSessao } from '../services/authApi'
import { listarTecnicos } from '../services/tecnicosApi'
import type {
    HistoricoSelecao,
    SelecaoTecnico
} from '../types/Matching'

export function SolicitacoesTecnico({ aoDiagnosticar }: { aoDiagnosticar: (id: number) => void }) {
    const sessao = obterSessao()
    if (sessao?.tipo !== 'Tecnico') {
        return <main className="pagina-conteudo"><p>Entre como técnico para ver suas solicitações.</p></main>
    }

    return <SolicitacoesRecebidas key={sessao.id} usuarioTecnicoId={sessao.id} aoDiagnosticar={aoDiagnosticar} />
}

function SolicitacoesRecebidas({ usuarioTecnicoId, aoDiagnosticar }: {
    usuarioTecnicoId: number
    aoDiagnosticar: (id: number) => void
}) {
    const [tecnicoId, setTecnicoId] = useState(0)
    const [selecoes, setSelecoes] =
        useState<SelecaoTecnico[]>([])
    const [historico, setHistorico] =
        useState<HistoricoSelecao[]>([])

    const [selecaoRecusadaId, setSelecaoRecusadaId] =
        useState<number | null>(null)
    const [motivo, setMotivo] = useState('')
    const [solicitacaoAceita, setSolicitacaoAceita] = useState<number>()
    const [carregando, setCarregando] = useState(true)
    const [processandoId, setProcessandoId] =
        useState<number | null>(null)

    const [toast, setToast] = useState<{
        mensagem: string
        tipo: 'sucesso' | 'erro'
    } | null>(null)

    useEffect(() => {
        let ativo = true

        async function carregar() {
            try {
                const perfis = await listarTecnicos()
                if (!ativo) return

                const perfil = perfis.find(item => item.usuarioId === usuarioTecnicoId)
                if (!perfil) return

                setTecnicoId(perfil.id)
                const dados = await listarSelecoesPendentes(perfil.id)
                if (ativo) setSelecoes(dados)
            } catch {
                if (ativo) setToast({ mensagem: 'Não foi possível carregar suas solicitações.', tipo: 'erro' })
            } finally {
                if (ativo) setCarregando(false)
            }
        }

        void carregar()
        return () => { ativo = false }
    }, [usuarioTecnicoId])

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

    async function aceitar(selecao: SelecaoTecnico) {
        setProcessandoId(selecao.id)

        try {
            await aceitarSelecao(selecao.id)
            setSolicitacaoAceita(selecao.solicitacaoId)
            await carregarPendentes(tecnicoId)
            await mostrarHistorico(selecao.id)

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
                {tecnicoId === 0 && !toast && (
                    <p className="estado-vazio">Sua conta ainda não tem um perfil técnico.</p>
                )}

                {tecnicoId > 0 && selecoes.length === 0 && (
                        <p className="estado-vazio">
                            Nenhuma solicitação aguardando resposta.
                        </p>
                    )}

                {solicitacaoAceita && (
                    <button
                        className="botao-principal"
                        type="button"
                        onClick={() => aoDiagnosticar(solicitacaoAceita)}
                    >
                        Ir para diagnóstico
                    </button>
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
                                        {selecao.solicitacaoNumero ?? 'Solicitação recebida'}
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
                                        aceitar(selecao)
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
                                    Iniciar recusa
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