import {
    useState,
    type FormEvent
} from 'react'
import { Toast } from '../components/Toast'
import {
    adicionarItemOrcamento,
    aprovarOrcamento,
    cancelarOrcamento,
    criarOrcamento,
    enviarOrcamento,
    obterOrcamentoPorDiagnostico,
    rejeitarOrcamento,
    removerItemOrcamento
} from '../services/orcamentosApi'
import type {
    Orcamento
} from '../types/DiagnosticoOrcamento'

type TipoToast = 'sucesso' | 'erro'

export function Orcamentos() {
    const [diagnosticoId, setDiagnosticoId] =
        useState('')

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
        useState(false)

    const [mensagem, setMensagem] =
        useState('')

    const [tipoMensagem, setTipoMensagem] =
        useState<TipoToast>('sucesso')

    const emElaboracao =
        orcamento?.status === 'EmElaboracao'

    const enviado =
        orcamento?.status === 'Enviado'

    function mostrarMensagem(
        texto: string,
        tipoToast: TipoToast
    ) {
        setMensagem(texto)
        setTipoMensagem(tipoToast)
    }

    function obterDiagnosticoId(): number | null {
        const id = Number(diagnosticoId)

        if (!Number.isInteger(id) || id <= 0) {
            mostrarMensagem(
                'Informe um ID de diagnóstico válido.',
                'erro'
            )

            return null
        }

        return id
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

    async function buscar(
        evento: FormEvent
    ) {
        evento.preventDefault()

        const id = obterDiagnosticoId()

        if (id === null) {
            return
        }

        setCarregando(true)

        try {
            const dados =
                await obterOrcamentoPorDiagnostico(id)

            setOrcamento(dados)

            mostrarMensagem(
                'Orçamento encontrado.',
                'sucesso'
            )
        } catch (erro) {
            setOrcamento(null)

            tratarErro(
                erro,
                'Não foi possível buscar o orçamento.'
            )
        } finally {
            setCarregando(false)
        }
    }

    async function criar() {
        const id = obterDiagnosticoId()

        if (id === null) {
            return
        }

        setCarregando(true)

        try {
            const dados =
                await criarOrcamento(id)

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

    async function adicionarItem(
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
            const dados =
                await adicionarItemOrcamento(
                    orcamento.id,
                    {
                        descricao: descricao.trim(),
                        tipo: tipo === '1' ? 1 : 2,
                        quantidade: quantidadeNumero,
                        valorUnitario: valorNumero
                    }
                )

            setOrcamento(dados)
            setDescricao('')
            setQuantidade('1')
            setValorUnitario('')

            mostrarMensagem(
                'Item adicionado.',
                'sucesso'
            )
        } catch (erro) {
            tratarErro(
                erro,
                'Não foi possível adicionar o item.'
            )
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
        <main className="pagina-equipamentos">
            {mensagem && (
                <Toast
                    mensagem={mensagem}
                    tipo={tipoMensagem}
                    aoFechar={() => setMensagem('')}
                />
            )}

            <div className="cabecalho-formulario">
                <span className="subtitulo-formulario">
                    PROPOSTA DE SERVIÇO
                </span>

                <h1>Orçamento</h1>

                <p>
                    Adicione peças e serviços e envie
                    a proposta para aprovação.
                </p>
            </div>

            <section className="painel-equipamentos">
                <form
                    className="formulario-equipamento"
                    onSubmit={buscar}
                >
                    <label>
                        ID do diagnóstico
                        <input
                            type="number"
                            min="1"
                            value={diagnosticoId}
                            onChange={(evento) =>
                                setDiagnosticoId(
                                    evento.target.value
                                )
                            }
                            placeholder="Ex.: 1"
                        />
                    </label>

                    <div className="acoes-equipamento">
                        <button
                            className="botao-secundario"
                            type="submit"
                            disabled={carregando}
                        >
                            Buscar orçamento
                        </button>

                        <button
                            className="botao-principal"
                            type="button"
                            disabled={carregando}
                            onClick={criar}
                        >
                            Criar orçamento
                        </button>
                    </div>
                </form>

                {orcamento && (
                    <>
                        <section className="confirmacao-solicitacao">
                            <span className="subtitulo-formulario">
                                RESUMO
                            </span>

                            <h2>
                                Orçamento #{orcamento.id}
                            </h2>

                            <dl>
                                <dt>Status</dt>
                                <dd>{orcamento.status}</dd>

                                <dt>Solicitação</dt>
                                <dd>
                                    {orcamento
                                        .solicitacaoNumero ||
                                        orcamento
                                            .solicitacaoId ||
                                        '-'}
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
                                onSubmit={adicionarItem}
                            >
                                <h2>Adicionar item</h2>

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
                                    Adicionar item
                                </button>
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

                                    {enviado && (
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

                                    <button
                                        className="botao-secundario"
                                        type="button"
                                        disabled={carregando}
                                        onClick={cancelar}
                                    >
                                        Cancelar orçamento
                                    </button>
                                </div>
                            </section>
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
        </main>
    )
}