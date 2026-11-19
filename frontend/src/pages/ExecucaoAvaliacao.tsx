import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { SeletorAtendimento } from '../components/SeletorAtendimento'
import type { Atendimento } from '../types/Atendimento'
import { obterSessao } from '../services/authApi'
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

export function ExecucaoAvaliacao({ idInicial }: { idInicial?: number }) {
    return (
        <main className="pagina-conteudo pagina-execucao">
            <div className="cabecalho-formulario">
                <h1>Execução e avaliação</h1>
                <p>Acompanhe o serviço aprovado e a avaliação do cliente.</p>
            </div>
            <SeletorAtendimento etapa="execucao" idInicial={idInicial}>
                {(atendimento, perfil) => (
                    <ExecucaoAtendimento
                        key={atendimento.id}
                        atendimento={atendimento}
                        tecnico={perfil === 'Tecnico'}
                    />
                )}
            </SeletorAtendimento>
        </main>
    )
}

function ExecucaoAtendimento({ atendimento, tecnico }: { atendimento: Atendimento; tecnico: boolean }) {
    const { orcamentoId } = atendimento
    const clienteDaSessao = !tecnico ? obterSessao()?.clienteId : undefined
    const [observacao, setObservacao] = useState('')
    const [nota, setNota] = useState(5)
    const [comentario, setComentario] = useState('')
    const [execucao, setExecucao] = useState<ExecucaoServico | null>(null)
    const [avaliacao, setAvaliacao] = useState<Avaliacao | null>(null)
    const [erro, setErro] = useState('')
    const [mensagem, setMensagem] = useState('')
    const [carregando, setCarregando] = useState(false)
    const [consultando, setConsultando] = useState(true)

    useEffect(() => {
        if (!orcamentoId) return
        let ativo = true

        async function carregar(id: number) {
            try {
                const encontrada = await obterExecucaoPorOrcamento(id)
                if (!ativo) return
                setExecucao(encontrada)

                if (encontrada?.status === 'Concluida') {
                    const notaRegistrada = await obterAvaliacaoPorExecucao(encontrada.id)
                    if (ativo) setAvaliacao(notaRegistrada)
                }
            } catch (falha) {
                if (ativo) setErro(falha instanceof Error ? falha.message : 'Não foi possível carregar a execução.')
            } finally {
                if (ativo) setConsultando(false)
            }
        }

        void carregar(orcamentoId)
        return () => { ativo = false }
    }, [orcamentoId])

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

    async function criar() {
        if (!orcamentoId || !tecnico) return
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
        if (!execucao || !clienteDaSessao) return
        await executar(async () => {
            const criada = await criarAvaliacao({
                execucaoServicoId: execucao.id,
                clienteId: clienteDaSessao,
                nota,
                comentario
            })
            setAvaliacao(criada)
            setMensagem('Avaliação registrada com sucesso.')
        })
    }

    return (
        <section className="painel-execucao">
            {consultando && <p>Carregando execução...</p>}
            {!consultando && !execucao && !erro && (
                tecnico ? (
                    <button className="botao-principal" type="button" disabled={carregando} onClick={criar}>
                        Criar execução
                    </button>
                ) : <p className="estado-vazio">Aguardando o técnico registrar a execução.</p>
            )}

            {erro && <p className="mensagem erro">{erro}</p>}
            {mensagem && <p className="mensagem sucesso">{mensagem}</p>}

            {execucao && (
                <div className="cartao-execucao">
                    <div className="resumo-execucao">
                        <span className="subtitulo-formulario">Resumo</span>
                        <h2>{atendimento.numero}</h2>
                        <p><strong>Status:</strong> {rotuloStatus(execucao.status)}</p>
                        <p><strong>Técnico:</strong> {execucao.tecnico ?? 'Não informado'}</p>
                        <p><strong>Início:</strong> {dataHora(execucao.dataInicio)}</p>
                        <p><strong>Conclusão:</strong> {dataHora(execucao.dataConclusao)}</p>
                    </div>

                    {tecnico && execucao.status !== 'Concluida' && (
                        <div className="acoes-execucao">
                            <label>
                                Observação
                                <textarea maxLength={1000} value={observacao}
                                    onChange={(e) => setObservacao(e.target.value)} />
                            </label>
                            {execucao.status === 'AguardandoInicio' && (
                                <button className="botao-principal" type="button" disabled={carregando} onClick={iniciar}>
                                    Iniciar serviço
                                </button>
                            )}
                            {execucao.status === 'EmAndamento' && (
                                <button className="botao-principal" type="button" disabled={carregando} onClick={concluir}>
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

            {clienteDaSessao && !consultando && execucao?.status === 'Concluida' && !avaliacao && (
                <form className="formulario-avaliacao" onSubmit={avaliar}>
                    <h2>Avaliar serviço</h2>
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
    )
}
