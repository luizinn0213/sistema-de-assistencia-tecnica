import { useEffect, useState } from 'react'
import { Toast } from '../components/Toast'
import { obterSessao } from '../services/authApi'
import { listarSolicitacoes } from '../services/solicitacoesApi'
import type { Solicitacao } from '../types/Solicitacao'

const TAMANHO_RESUMO = 100

function resumirDescricao(descricao: string) {
    if (descricao.length <= TAMANHO_RESUMO) {
        return descricao
    }

    return `${descricao.slice(0, TAMANHO_RESUMO).trimEnd()}...`
}

export function MinhasSolicitacoes() {
    const sessao = obterSessao()
    const clienteId = sessao?.clienteId

    const [solicitacoes, setSolicitacoes] =
        useState<Solicitacao[]>([])

    const [carregando, setCarregando] =
        useState(Boolean(clienteId))

    const [erro, setErro] = useState('')

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
                    <article
                        className="cartao-equipamento"
                        key={solicitacao.id}
                    >
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
                    </article>
                ))}
            </section>
        </main>
    )
}
