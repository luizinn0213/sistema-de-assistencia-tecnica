import { useState } from 'react'
import type { FormEvent } from 'react'
import {
    concluirExecucao,
    criarAvaliacao,
    criarExecucao,
    iniciarExecucao,
    obterAvaliacaoPorExecucao,
    obterExecucaoPorOrcamento
} from '../services/execucoesApi'
import type {
    Avaliacao,
    ExecucaoServico
} from '../types/ExecucaoAvaliacao'

function dataHora(valor?: string) {
    return valor ? new Date(valor).toLocaleString('pt-BR') : '—'
}

function rotuloStatus(status: string): string {
    const rotulos: Record<string, string> = {
        AguardandoInicio: 'Aguardando início',
        EmAndamento: 'Em andamento',
        Concluida: 'Concluída'
    }

    return rotulos[status] ?? status
}

export function ExecucaoAvaliacao() {
    const [orcamentoId, setOrcamentoId] = useState(1)
    const [clienteId, setClienteId] = useState(1)
    const [observacao, setObservacao] = useState('')
    const [nota, setNota] = useState(5)
    const [comentario, setComentario] = useState('')
    const [execucao, setExecucao] = useState<ExecucaoServico | null>(null)
    const [avaliacao, setAvaliacao] = useState<Avaliacao | null>(null)
    const [erro, setErro] = useState('')
    const [mensagem, setMensagem] = useState('')
    const [carregando, setCarregando] = useState(false)

    async function executar(acao: () => Promise<void>) {
        setCarregando(true)
        setErro('')
        setMensagem('')
        try {
            await acao()
        } catch (recebido) {
            setErro(
                recebido instanceof Error
                    ? recebido.message
                    : 'Não foi possível concluir a operação.'
            )
        } finally {
            setCarregando(false)
        }
    }

    async function buscar(evento: FormEvent) {
        evento.preventDefault()
        await executar(async () => {
            const encontrada = await obterExecucaoPorOrcamento(orcamentoId)
            setExecucao(encontrada)
            setAvaliacao(null)

            if (encontrada.status === 'Concluida') {
                try {
                    setAvaliacao(await obterAvaliacaoPorExecucao(encontrada.id))
                } catch {
                    setAvaliacao(null)
                }
            }
        })
    }

    async function criar() {
        await executar(async () => {
            setExecucao(await criarExecucao(orcamentoId))
            setMensagem('Execução criada. O serviço já pode ser iniciado.')
        })
    }

    async function iniciar() {
        if (!execucao) return
        await executar(async () => {
            setExecucao(await iniciarExecucao(execucao.id, observacao))
            setObservacao('')
            setMensagem('Execução iniciada.')
        })
    }

    async function concluir() {
        if (!execucao) return
        await executar(async () => {
            setExecucao(await concluirExecucao(execucao.id, observacao))
            setObservacao('')
            setMensagem('Serviço concluído. A avaliação foi liberada.')
        })
    }

    async function avaliar(evento: FormEvent) {
        evento.preventDefault()
        if (!execucao) return
        await executar(async () => {
            const criada = await criarAvaliacao({
                execucaoServicoId: execucao.id,
                clienteId,
                nota,
                comentario
            })
            setAvaliacao(criada)
            setMensagem('Avaliação registrada com sucesso.')
        })
    }

    return (
        <main className="pagina-conteudo pagina-execucao">
            <div className="cabecalho-formulario">
                <span className="subtitulo-formulario">Fluxo do atendimento</span>
                <h1>Execução e avaliação</h1>
                <p>Inicie e conclua o serviço aprovado e registre a avaliação do cliente.</p>
            </div>

            <section className="painel-execucao">
                <form className="formulario-execucao" onSubmit={buscar}>
                    <label>
                        ID do orçamento aprovado
                        <input type="number" min="1" value={orcamentoId}
                            onChange={(e) => setOrcamentoId(Number(e.target.value))} />
                    </label>
                    <button className="botao-secundario" disabled={carregando}>
                        Buscar execução
                    </button>
                    <button className="botao-principal" type="button"
                        disabled={carregando} onClick={criar}>
                        Criar execução
                    </button>
                </form>

                {erro && <p className="mensagem erro">{erro}</p>}
                {mensagem && <p className="mensagem sucesso">{mensagem}</p>}

                {execucao && (
                    <div className="cartao-execucao">
                        <div className="resumo-execucao">
                            <span className="subtitulo-formulario">Resumo</span>
                            <h2>Execução #{execucao.id}</h2>
                            <p><strong>Status:</strong> {rotuloStatus(execucao.status)}</p>
                            <p><strong>Solicitação:</strong> {execucao.solicitacaoNumero ?? execucao.solicitacaoId}</p>
                            <p><strong>Técnico:</strong> {execucao.tecnico ?? 'Não informado'}</p>
                            <p><strong>Início:</strong> {dataHora(execucao.dataInicio)}</p>
                            <p><strong>Conclusão:</strong> {dataHora(execucao.dataConclusao)}</p>
                        </div>

                        {execucao.status !== 'Concluida' && (
                            <div className="acoes-execucao">
                                <label>
                                    Observação
                                    <textarea maxLength={1000} value={observacao}
                                        onChange={(e) => setObservacao(e.target.value)} />
                                </label>
                                {execucao.status === 'AguardandoInicio' && (
                                    <button className="botao-principal" type="button" onClick={iniciar}>
                                        Iniciar serviço
                                    </button>
                                )}
                                {execucao.status === 'EmAndamento' && (
                                    <button className="botao-principal" type="button" onClick={concluir}>
                                        Concluir serviço
                                    </button>
                                )}
                            </div>
                        )}

                        <div className="historico-execucao">
                            <h2>Histórico</h2>
                            {execucao.historico.map((item) => (
                                <article key={`${item.id}-${item.dataRegistro}`}>
                                    <strong>{rotuloStatus(item.status)}</strong>
                                    <p>{item.observacao}</p>
                                    <small>{dataHora(item.dataRegistro)}</small>
                                </article>
                            ))}
                        </div>
                    </div>
                )}

                {execucao?.status === 'Concluida' && !avaliacao && (
                    <form className="formulario-avaliacao" onSubmit={avaliar}>
                        <h2>Avaliar serviço</h2>
                        <label>
                            ID do cliente
                            <input type="number" min="1" value={clienteId}
                                onChange={(e) => setClienteId(Number(e.target.value))} />
                        </label>
                        <label>
                            Nota
                            <select value={nota} onChange={(e) => setNota(Number(e.target.value))}>
                                {[1, 2, 3, 4, 5].map((valor) => (
                                    <option key={valor} value={valor}>{valor} estrela{valor > 1 ? 's' : ''}</option>
                                ))}
                            </select>
                        </label>
                        <label>
                            Comentário
                            <textarea maxLength={1000} value={comentario}
                                onChange={(e) => setComentario(e.target.value)} />
                        </label>
                        <button className="botao-principal" disabled={carregando}>
                            Enviar avaliação
                        </button>
                    </form>
                )}

                {avaliacao && (
                    <div className="avaliacao-registrada">
                        <h2>Avaliação registrada</h2>
                        <p className="estrelas-avaliacao">{'★'.repeat(avaliacao.nota)}</p>
                        <p>{avaliacao.comentario || 'Sem comentário.'}</p>
                        <small>{dataHora(avaliacao.dataAvaliacao)}</small>
                    </div>
                )}
            </section>
        </main>
    )
}
