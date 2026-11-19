import { useEffect, useState } from 'react'
import { Toast } from '../components/Toast'
import { obterSessao } from '../services/authApi'
import { cancelarSolicitacao, listarHistoricoSolicitacao, listarSolicitacoes } from '../services/solicitacoesApi'
import type { HistoricoSolicitacao, Solicitacao } from '../types/Solicitacao'

const TAMANHO_RESUMO = 100

function resumirDescricao(descricao: string) {
    if (descricao.length <= TAMANHO_RESUMO) {
        return descricao
    }

    return `${descricao.slice(0, TAMANHO_RESUMO).trimEnd()}...`
}

export function MinhasSolicitacoes({
    aoEscolherTecnico
}: {
    aoEscolherTecnico: (solicitacao: Solicitacao) => void
}) {
    const sessao = obterSessao()
    const clienteId = sessao?.clienteId

    const [solicitacoes, setSolicitacoes] =
        useState<Solicitacao[]>([])

    const [carregando, setCarregando] =
        useState(Boolean(clienteId))

    const [erro, setErro] = useState('')
    const [aberta, setAberta] = useState<number>()
    const [historico, setHistorico] = useState<HistoricoSolicitacao[] | null>(null)
    const [processando, setProcessando] = useState(false)

    async function abrirHistorico(solicitacao: Solicitacao) {
        if (!clienteId) return
        if (aberta === solicitacao.id) {
            setAberta(undefined)
            return
        }

        setProcessando(true)
        setAberta(solicitacao.id)
        setHistorico(null)
        try {
            setHistorico(await listarHistoricoSolicitacao(clienteId, solicitacao.id))
        } catch (falha) {
            setAberta(undefined)
            setErro(falha instanceof Error ? falha.message : 'Não foi possível carregar o histórico.')
        } finally {
            setProcessando(false)
        }
    }

    async function cancelar(solicitacao: Solicitacao) {
        if (!clienteId || !window.confirm(`Cancelar a solicitação ${solicitacao.numero}?`)) return

        setProcessando(true)
        try {
            const alterada = await cancelarSolicitacao(clienteId, solicitacao.id)
            setSolicitacoes(dados => dados.map(item => item.id === alterada.id ? alterada : item))
            setAberta(alterada.id)
            setHistorico(null)
            setHistorico(await listarHistoricoSolicitacao(clienteId, alterada.id))
        } catch (falha) {
            setErro(falha instanceof Error ? falha.message : 'Não foi possível cancelar a solicitação.')
        } finally {
            setProcessando(false)
        }
    }

    useEffect(() => {
        if (!clienteId) {
            return
        }

        let componenteAtivo = true

        listarSolicitacoes(clienteId)
            .then((dados) => {
                if (componenteAtivo) {
                    setSolicitacoes(dados)
                }
            })
            .catch((erroRecebido) => {
                if (!componenteAtivo) {
                    return
                }

                if (erroRecebido instanceof Error) {
                    setErro(erroRecebido.message)
                } else {
                    setErro(
                        'Não foi possível carregar as solicitações.'
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
    }, [clienteId])

    if (!clienteId) {
        return (
            <main className="pagina-conteudo">
                <div className="cabecalho-formulario">
                    <span className="subtitulo-formulario">
                        Área do cliente
                    </span>

                    <h1>Minhas solicitações</h1>

                    <p>
                        Faça login como cliente para visualizar
                        suas solicitações.
                    </p>
                </div>
            </main>
        )
    }

    return (
        <main className="pagina-equipamentos">
            {erro && (
                <Toast
                    mensagem={erro}
                    tipo="erro"
                    aoFechar={() => setErro('')}
                />
            )}

            <div className="cabecalho-formulario">
                <span className="subtitulo-formulario">
                    Área do cliente
                </span>

                <h1>Minhas solicitações</h1>

                <p>
                    Acompanhe as solicitações de assistência
                    que você abriu.
                </p>
            </div>

            <section className="lista-equipamentos lista-solicitacoes">
                <div className="titulo-lista-equipamentos">
                    <div>
                        <span>Seus pedidos</span>

                        <h2>Solicitações abertas</h2>
                    </div>

                    <strong>{solicitacoes.length}</strong>
                </div>

                {carregando && (
                    <p>Carregando solicitações...</p>
                )}

                {!carregando &&
                    solicitacoes.length === 0 && (
                        <p className="estado-vazio">
                            Nenhuma solicitação aberta.
                        </p>
                    )}

                {solicitacoes.map((solicitacao) => (
                    <section key={solicitacao.id}>
                        <article className="cartao-equipamento">
                            <div className="icone-equipamento">
                                {solicitacao.equipamento.tipo
                                    .charAt(0)
                                    .toUpperCase()}
                            </div>

                            <div className="dados-equipamento">
                                <span>{solicitacao.numero}</span>

                                <h3>
                                    {solicitacao.equipamento.tipo}{' '}
                                    {solicitacao.equipamento.marca}{' '}
                                    {solicitacao.equipamento.modelo}
                                </h3>

                                <p>
                                    {resumirDescricao(
                                        solicitacao.descricaoProblema
                                    )}
                                </p>

                                <p>
                                    {solicitacao.especialidade.nome}
                                    {' · '}
                                    {new Date(
                                        solicitacao.dataCriacao
                                    ).toLocaleString('pt-BR')}
                                </p>
                            </div>

                            <span className="status-solicitacao">
                                {solicitacao.status}
                            </span>

                            {['Aberta', 'AguardandoTecnico'].includes(solicitacao.status) && (
                                <button
                                    className="botao-secundario"
                                    type="button"
                                    onClick={() => aoEscolherTecnico(solicitacao)}
                                >
                                    Escolher técnico
                                </button>
                            )}
                        </article>
                        <div className="acoes-equipamento">
                            <button
                                className="botao-secundario"
                                type="button"
                                disabled={processando}
                                onClick={() => abrirHistorico(solicitacao)}
                            >
                                {aberta === solicitacao.id ? 'Fechar histórico' : 'Ver histórico'}
                            </button>
                            {!['Cancelada', 'EmExecucao', 'Concluida'].includes(solicitacao.status) && (
                                <button
                                    className="botao-perigo"
                                    type="button"
                                    disabled={processando}
                                    onClick={() => cancelar(solicitacao)}
                                >
                                    Cancelar solicitação
                                </button>
                            )}
                        </div>
                        {aberta === solicitacao.id && (
                            <div className="painel-historico">
                                <h3>Histórico da solicitação</h3>
                                {historico === null && processando && <p>Carregando histórico...</p>}
                                {historico?.length === 0 && <p>A solicitação ainda não mudou de status.</p>}
                                {historico && historico.length > 0 && (
                                    <ol>
                                        {historico.map(item => (
                                            <li key={item.id}>
                                                {item.statusAnterior} → {item.novoStatus} ·{' '}
                                                <time dateTime={item.dataAlteracao}>
                                                    {new Date(item.dataAlteracao).toLocaleString('pt-BR')}
                                                </time>
                                            </li>
                                        ))}
                                    </ol>
                                )}
                            </div>
                        )}
                    </section>
                ))}
            </section>
        </main>
    )
}
