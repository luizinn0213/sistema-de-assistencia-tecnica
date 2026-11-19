import {
    useEffect,
    useState,
    type FormEvent
} from 'react'
import { SeletorAtendimento } from '../components/SeletorAtendimento'
import type { Atendimento } from '../types/Atendimento'
import { Toast } from '../components/Toast'
import {
    adicionarItemOrcamento,
    alterarItemOrcamento,
    aprovarOrcamento,
    cancelarOrcamento,
    criarOrcamento,
    enviarOrcamento,
    obterOrcamentoPorDiagnostico,
    rejeitarOrcamento,
    removerItemOrcamento
} from '../services/orcamentosApi'
import type {
    ItemOrcamento,
    ItemOrcamentoDados,
    Orcamento
} from '../types/DiagnosticoOrcamento'

type TipoToast = 'sucesso' | 'erro'

export function Orcamentos({ idInicial, aoAcompanhar }: { idInicial?: number; aoAcompanhar: (id: number) => void }) {
    return (
        <main className="pagina-equipamentos">
            <div className="cabecalho-formulario">
                <h1>Orçamento</h1>
                <p>Prepare ou acompanhe a proposta do atendimento.</p>
            </div>
            <SeletorAtendimento etapa="orcamento" idInicial={idInicial}>
                {(atendimento, perfil) => (
                    <OrcamentoAtendimento
                        key={atendimento.id}
                        atendimento={atendimento}
                        tecnico={perfil === 'Tecnico'}
                        aoAcompanhar={() => aoAcompanhar(atendimento.id)}
                    />
                )}
            </SeletorAtendimento>
        </main>
    )
}

function OrcamentoAtendimento({ atendimento, tecnico, aoAcompanhar }: {
    atendimento: Atendimento
    tecnico: boolean
    aoAcompanhar: () => void
}) {
    const { diagnosticoId, orcamentoId } = atendimento
    const [itemEmEdicao, setItemEmEdicao] = useState<number>()

    const [orcamento, setOrcamento] =
        useState<Orcamento | null>(null)

    const [descricao, setDescricao] =
        useState('')

    const [tipo, setTipo] =
        useState<'1' | '2'>('1')

    const [quantidade, setQuantidade] =
        useState('1')

    const [valorUnitario, setValorUnitario] =
        useState('')

    const [observacao, setObservacao] =
        useState('')

    const [carregando, setCarregando] =
        useState(Boolean(orcamentoId))

    const [mensagem, setMensagem] =
        useState('')

    const [tipoMensagem, setTipoMensagem] =
        useState<TipoToast>('sucesso')

    const emElaboracao =
        tecnico && orcamento?.status === 'EmElaboracao'

    const enviado =
        orcamento?.status === 'Enviado'

    function mostrarMensagem(
        texto: string,
        tipoToast: TipoToast
    ) {
        setMensagem(texto)
        setTipoMensagem(tipoToast)
    }

    function tratarErro(
        erro: unknown,
        mensagemPadrao: string
    ) {
        mostrarMensagem(
            erro instanceof Error
                ? erro.message
                : mensagemPadrao,
            'erro'
        )
    }

    useEffect(() => {
        if (!diagnosticoId || !orcamentoId) return
        let ativo = true

        obterOrcamentoPorDiagnostico(diagnosticoId).then(dados => {
            if (ativo) setOrcamento(dados)
        }).catch((erro: unknown) => {
            if (ativo) {
                setMensagem(erro instanceof Error ? erro.message : 'Não foi possível carregar o orçamento.')
                setTipoMensagem('erro')
            }
        }).finally(() => {
            if (ativo) setCarregando(false)
        })

        return () => { ativo = false }
    }, [diagnosticoId, orcamentoId])

    async function criar() {
        if (!tecnico || !diagnosticoId) return

        setCarregando(true)

        try {
            const dados =
                await criarOrcamento(diagnosticoId)

            setOrcamento(dados)

            mostrarMensagem(
                'Orçamento criado com sucesso.',
                'sucesso'
            )
        } catch (erro) {
            tratarErro(
                erro,
                'Não foi possível criar o orçamento.'
            )
        } finally {
            setCarregando(false)
        }
    }

    function editarItem(item: ItemOrcamento) {
        setItemEmEdicao(item.id)
        setDescricao(item.descricao)
        setTipo(item.tipo === 'Peca' ? '1' : '2')
        setQuantidade(String(item.quantidade))
        setValorUnitario(String(item.valorUnitario))
    }

    function limparItem() {
        setItemEmEdicao(undefined)
        setDescricao('')
        setTipo('1')
        setQuantidade('1')
        setValorUnitario('')
    }

    async function salvarItem(
        evento: FormEvent
    ) {
        evento.preventDefault()

        if (!orcamento) {
            return
        }

        const quantidadeNumero =
            Number(quantidade)

        const valorNumero =
            Number(valorUnitario)

        if (!descricao.trim()) {
            mostrarMensagem(
                'Informe a descrição do item.',
                'erro'
            )

            return
        }

        if (
            !Number.isInteger(quantidadeNumero) ||
            quantidadeNumero <= 0
        ) {
            mostrarMensagem(
                'Informe uma quantidade válida.',
                'erro'
            )

            return
        }

        if (
            !Number.isFinite(valorNumero) ||
            valorNumero < 0
        ) {
            mostrarMensagem(
                'Informe um valor válido.',
                'erro'
            )

            return
        }

        setCarregando(true)

        try {
            const item: ItemOrcamentoDados = {
                descricao: descricao.trim(),
                tipo: tipo === '1' ? 1 : 2,
                quantidade: quantidadeNumero,
                valorUnitario: valorNumero
            }

            const dados = itemEmEdicao
                ? await alterarItemOrcamento(orcamento.id, itemEmEdicao, item)
                : await adicionarItemOrcamento(orcamento.id, item)

            setOrcamento(dados)
            mostrarMensagem(itemEmEdicao ? 'Item atualizado.' : 'Item adicionado.', 'sucesso')
            limparItem()
        } catch (erro) {
            tratarErro(erro, 'Não foi possível salvar o item.')
        } finally {
            setCarregando(false)
        }
    }

    async function removerItem(
        itemId: number
    ) {
        if (!orcamento) {
            return
        }

        const confirmou = window.confirm(
            'Deseja remover este item?'
        )

        if (!confirmou) {
            return
        }

        setCarregando(true)

        try {
            const dados =
                await removerItemOrcamento(
                    orcamento.id,
                    itemId
                )

            setOrcamento(dados)
            if (itemId === itemEmEdicao) limparItem()

            mostrarMensagem(
                'Item removido.',
                'sucesso'
            )
        } catch (erro) {
            tratarErro(
                erro,
                'Não foi possível remover o item.'
            )
        } finally {
            setCarregando(false)
        }
    }

    async function enviar() {
        if (!orcamento) {
            return
        }

        setCarregando(true)

        try {
            const dados =
                await enviarOrcamento(
                    orcamento.id
                )

            setOrcamento(dados)

            mostrarMensagem(
                'Orçamento enviado ao cliente.',
                'sucesso'
            )
        } catch (erro) {
            tratarErro(
                erro,
                'Não foi possível enviar o orçamento.'
            )
        } finally {
            setCarregando(false)
        }
    }

    async function responder(
        operacao: 'aprovar' | 'rejeitar'
    ) {
        if (!orcamento) {
            return
        }

        setCarregando(true)

        try {
            const dados =
                operacao === 'aprovar'
                    ? await aprovarOrcamento(
                          orcamento.id,
                          { observacao }
                      )
                    : await rejeitarOrcamento(
                          orcamento.id,
                          { observacao }
                      )

            setOrcamento(dados)
            setObservacao('')

            mostrarMensagem(
                operacao === 'aprovar'
                    ? 'Orçamento aprovado.'
                    : 'Orçamento rejeitado.',
                'sucesso'
            )
        } catch (erro) {
            tratarErro(
                erro,
                'Não foi possível responder ao orçamento.'
            )
        } finally {
            setCarregando(false)
        }
    }

    async function cancelar() {
        if (!orcamento) {
            return
        }

        setCarregando(true)

        try {
            const dados =
                await cancelarOrcamento(
                    orcamento.id,
                    { observacao }
                )

            setOrcamento(dados)
            setObservacao('')

            mostrarMensagem(
                'Orçamento cancelado.',
                'sucesso'
            )
        } catch (erro) {
            tratarErro(
                erro,
                'Não foi possível cancelar o orçamento.'
            )
        } finally {
            setCarregando(false)
        }
    }

    function formatarMoeda(
        valor: number
    ) {
        return new Intl.NumberFormat(
            'pt-BR',
            {
                style: 'currency',
                currency: 'BRL'
            }
        ).format(valor)
    }

    function formatarData(
        valor?: string
    ) {
        if (!valor) {
            return '-'
        }

        return new Date(valor).toLocaleString(
            'pt-BR'
        )
    }

    return (
        <>
            {mensagem && (
                <Toast
                    mensagem={mensagem}
                    tipo={tipoMensagem}
                    aoFechar={() => setMensagem('')}
                />
            )}

            <section className="painel-equipamentos">
                {orcamentoId && !orcamento && carregando && <p>Carregando orçamento...</p>}
                {tecnico && !orcamentoId && !orcamento && (
                    <button className="botao-principal" type="button" disabled={carregando} onClick={criar}>
                        Criar orçamento
                    </button>
                )}

                {orcamento && (
                    <>
                        <section className="confirmacao-solicitacao">
                            <span className="subtitulo-formulario">
                                RESUMO
                            </span>

                            <h2>
                                Proposta do atendimento
                            </h2>

                            <dl>
                                <dt>Status</dt>
                                <dd>{orcamento.status}</dd>

                                <dt>Solicitação</dt>
                                <dd>
                                    {atendimento.numero}
                                </dd>

                                <dt>Criado em</dt>
                                <dd>
                                    {formatarData(
                                        orcamento.dataCriacao
                                    )}
                                </dd>

                                <dt>Total</dt>
                                <dd>
                                    <strong>
                                        {formatarMoeda(
                                            orcamento.valorTotal
                                        )}
                                    </strong>
                                </dd>
                            </dl>
                        </section>

                        {emElaboracao && (
                            <form
                                className="formulario-equipamento"
                                onSubmit={salvarItem}
                            >
                                <h2>{itemEmEdicao ? 'Editar item' : 'Adicionar item'}</h2>

                                <label>
                                    Descrição
                                    <input
                                        value={descricao}
                                        maxLength={500}
                                        onChange={(evento) =>
                                            setDescricao(
                                                evento.target
                                                    .value
                                            )
                                        }
                                        placeholder="Peça ou serviço"
                                    />
                                </label>

                                <label>
                                    Tipo
                                    <select
                                        value={tipo}
                                        onChange={(evento) =>
                                            setTipo(
                                                evento.target
                                                    .value as
                                                    | '1'
                                                    | '2'
                                            )
                                        }
                                    >
                                        <option value="1">
                                            Peça
                                        </option>
                                        <option value="2">
                                            Serviço
                                        </option>
                                    </select>
                                </label>

                                <label>
                                    Quantidade
                                    <input
                                        type="number"
                                        min="1"
                                        value={quantidade}
                                        onChange={(evento) =>
                                            setQuantidade(
                                                evento.target
                                                    .value
                                            )
                                        }
                                    />
                                </label>

                                <label>
                                    Valor unitário
                                    <input
                                        type="number"
                                        min="0"
                                        step="0.01"
                                        value={valorUnitario}
                                        onChange={(evento) =>
                                            setValorUnitario(
                                                evento.target
                                                    .value
                                            )
                                        }
                                        placeholder="0,00"
                                    />
                                </label>

                                <button
                                    className="botao-principal"
                                    type="submit"
                                    disabled={carregando}
                                >
                                    {itemEmEdicao ? 'Salvar item' : 'Adicionar item'}
                                </button>
                                {itemEmEdicao && (
                                    <button className="botao-secundario" type="button" onClick={limparItem}>
                                        Cancelar edição
                                    </button>
                                )}
                            </form>
                        )}

                        <section className="lista-equipamentos">
                            <div className="titulo-lista-equipamentos">
                                <h2>Itens</h2>

                                <strong>
                                    {formatarMoeda(
                                        orcamento.valorTotal
                                    )}
                                </strong>
                            </div>

                            {orcamento.itens.length === 0 ? (
                                <p className="estado-vazio">
                                    Nenhum item adicionado.
                                </p>
                            ) : (
                                orcamento.itens.map(
                                    (item) => (
                                        <article
                                            className="cartao-equipamento"
                                            key={item.id}
                                        >
                                            <div className="dados-equipamento">
                                                <h3>
                                                    {
                                                        item.descricao
                                                    }
                                                </h3>

                                                <p>
                                                    {item.tipo} ·{' '}
                                                    {
                                                        item.quantidade
                                                    }{' '}
                                                    ×{' '}
                                                    {formatarMoeda(
                                                        item.valorUnitario
                                                    )}
                                                </p>

                                                <strong>
                                                    {formatarMoeda(
                                                        item.valorTotal
                                                    )}
                                                </strong>
                                            </div>

                                            {emElaboracao && (
                                                <div className="acoes-equipamento">
                                                    <button
                                                        className="botao-secundario"
                                                        type="button"
                                                        disabled={carregando}
                                                        onClick={() => editarItem(item)}
                                                    >
                                                        Editar item
                                                    </button>
                                                    <button
                                                        className="acao-perigosa"
                                                        type="button"
                                                        disabled={
                                                            carregando
                                                        }
                                                        onClick={() =>
                                                            removerItem(
                                                                item.id
                                                            )
                                                        }
                                                    >
                                                        Remover
                                                    </button>
                                                </div>
                                            )}
                                        </article>
                                    )
                                )
                            )}
                        </section>

                        {(emElaboracao || enviado) && (
                            <section className="formulario-equipamento">
                                <label>
                                    Observação
                                    <textarea
                                        rows={3}
                                        maxLength={900}
                                        value={observacao}
                                        onChange={(evento) =>
                                            setObservacao(
                                                evento.target
                                                    .value
                                            )
                                        }
                                        placeholder="Observação opcional"
                                    />
                                </label>

                                <div className="acoes-equipamento">
                                    {emElaboracao && (
                                        <button
                                            className="botao-principal"
                                            type="button"
                                            disabled={carregando}
                                            onClick={enviar}
                                        >
                                            Enviar orçamento
                                        </button>
                                    )}

                                    {enviado && !tecnico && (
                                        <>
                                            <button
                                                className="botao-principal"
                                                type="button"
                                                disabled={carregando}
                                                onClick={() =>
                                                    responder(
                                                        'aprovar'
                                                    )
                                                }
                                            >
                                                Aprovar
                                            </button>

                                            <button
                                                className="botao-perigo"
                                                type="button"
                                                disabled={carregando}
                                                onClick={() =>
                                                    responder(
                                                        'rejeitar'
                                                    )
                                                }
                                            >
                                                Rejeitar
                                            </button>
                                        </>
                                    )}

                                    {tecnico && (
                                        <button
                                            className="botao-secundario"
                                            type="button"
                                            disabled={carregando}
                                            onClick={cancelar}
                                        >
                                            Cancelar orçamento
                                        </button>
                                    )}
                                </div>
                            </section>
                        )}

                        {orcamento.status === 'Aprovado' && (
                            <button className="botao-principal" type="button" onClick={aoAcompanhar}>
                                Acompanhar execução
                            </button>
                        )}

                        <section className="painel-historico">
                            <h2>Histórico</h2>

                            <div className="linha-do-tempo">
                                {orcamento.historico.map(
                                    (item) => (
                                        <article key={item.id}>
                                            <strong>
                                                {item.status}
                                            </strong>

                                            <p>
                                                {
                                                    item.observacao
                                                }
                                            </p>

                                            <small>
                                                {formatarData(
                                                    item.dataRegistro
                                                )}
                                            </small>
                                        </article>
                                    )
                                )}
                            </div>
                        </section>
                    </>
                )}
            </section>
        </>
    )
}