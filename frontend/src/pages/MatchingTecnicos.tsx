import { useState } from 'react'
import type { FormEvent } from 'react'
import { Toast } from '../components/Toast'
import { SeletorAtendimento } from '../components/SeletorAtendimento'
import {
    buscarRankingTecnicos,
    buscarSelecaoDaSolicitacao,
    selecionarTecnico
} from '../services/matchingApi'
import type { Atendimento } from '../types/Atendimento'
import type {
    SelecaoTecnico,
    TecnicoRanking
} from '../types/Matching'

const NOMES_NIVEL: Record<number, string> = {
    0: 'Iniciante',
    1: 'Intermediário',
    2: 'Avançado',
    3: 'Especialista'
}

function formatarNivel(nivel: number | string): string {
    if (typeof nivel === 'number') {
        return NOMES_NIVEL[nivel] ?? String(nivel)
    }
    return nivel
}

export function MatchingTecnicos({ idInicial }: { idInicial?: number }) {
    return (
        <main className="pagina-conteudo pagina-matching">
            <div className="cabecalho-formulario">
                <h1>Encontrar técnico</h1>
                <p>Compare os técnicos disponíveis para a especialidade da solicitação.</p>
            </div>
            <SeletorAtendimento etapa="matching" idInicial={idInicial}>
                {atendimento => <RankingAtendimento key={atendimento.id} atendimento={atendimento} />}
            </SeletorAtendimento>
        </main>
    )
}

function RankingAtendimento({ atendimento }: { atendimento: Atendimento }) {
    const { id: solicitacaoId, especialidade } = atendimento
    const [cidade, setCidade] = useState(atendimento.cidade ?? '')

    const [tecnicos, setTecnicos] =
        useState<TecnicoRanking[]>([])
    const [buscaRealizada, setBuscaRealizada] = useState(false)

    const [carregando, setCarregando] = useState(false)
    const [selecionandoId, setSelecionandoId] =
        useState<number | null>(null)

    const [erro, setErro] = useState('')
    const [selecao, setSelecao] =
        useState<SelecaoTecnico | null>(null)
    const [toast, setToast] = useState<{
        mensagem: string
        tipo: 'sucesso' | 'erro'
    } | null>(null)

    const selecaoAtiva = selecao?.status === 'AguardandoResposta' ||
        selecao?.status === 'Aceita'
    const ocupado = carregando || selecionandoId !== null

    function limparBusca() {
        setSelecao(null)
        setTecnicos([])
        setBuscaRealizada(false)
        setErro('')
    }

    async function executarBusca() {
        setCarregando(true)
        limparBusca()

        try {
            const [ranking, selecaoAtual] = await Promise.all([
                buscarRankingTecnicos(especialidade.id, cidade, solicitacaoId),
                buscarSelecaoDaSolicitacao(solicitacaoId)
            ])

            setSelecao(selecaoAtual)
            setTecnicos(ranking)
            setBuscaRealizada(true)
        } catch (erroRecebido) {
            const mensagem =
                erroRecebido instanceof Error
                    ? erroRecebido.message
                    : 'Não foi possível buscar os técnicos.'
            setErro(mensagem)
            setToast({ mensagem, tipo: 'erro' })
        } finally {
            setCarregando(false)
        }
    }

    async function buscarTecnicos(
        evento: FormEvent<HTMLFormElement>
    ) {
        evento.preventDefault()
        await executarBusca()
    }

    async function escolherTecnico(
        tecnico: TecnicoRanking
    ) {
        setSelecionandoId(tecnico.id)
        setErro('')

        try {
            const resultado = await selecionarTecnico({
                solicitacaoId,
                tecnicoId: tecnico.id,
                especialidadeId: tecnico.especialidadeId
            })

            setSelecao(resultado)
            setToast({
                mensagem: `Técnico ${tecnico.nomeExibicao} selecionado com sucesso!`,
                tipo: 'sucesso'
            })
        } catch (erroRecebido) {
            const mensagem =
                erroRecebido instanceof Error
                    ? erroRecebido.message
                    : 'Não foi possível selecionar o técnico.'
            setErro(mensagem)
            setToast({ mensagem, tipo: 'erro' })
        } finally {
            setSelecionandoId(null)
        }
    }

    return (
        <>
            {toast && (
                <Toast
                    mensagem={toast.mensagem}
                    tipo={toast.tipo}
                    aoFechar={() => setToast(null)}
                />
            )}

            <form className="formulario-matching" onSubmit={buscarTecnicos}>
                <p>Especialidade: <strong>{especialidade.nome}</strong></p>

                <label>
                    Cidade do atendimento

                    <input
                        type="text"
                        value={cidade}
                        placeholder="Ex.: Teresópolis"
                        disabled={ocupado}
                        onChange={(evento) => {
                            setCidade(evento.target.value)
                            limparBusca()
                        }}
                    />
                </label>

                <button
                    className="botao-principal"
                    type="submit"
                    disabled={ocupado}
                >
                    {carregando
                        ? 'Buscando...'
                        : 'Buscar técnicos'}
                </button>
            </form>

            {erro && (
                <p className="mensagem erro">{erro}</p>
            )}

            {selecao && (
                <div className="mensagem sucesso">
                    <strong>
                        {selecaoAtiva ? 'Técnico selecionado' : 'Última seleção'}
                    </strong>

                    <span>
                        {selecao.tecnico} ·{' '}
                        {selecao.especialidade}
                    </span>

                    <span>
                        {selecao.status === 'Aceita' ? 'Aceita' :
                            selecao.status === 'Recusada' ? 'Recusada' : 'Aguardando resposta'}
                    </span>

                    {selecao.status === 'AguardandoResposta' && (
                        <button
                            type="button"
                            className="botao-secundario"
                            disabled={ocupado}
                            onClick={executarBusca}
                        >
                            Atualizar resposta
                        </button>
                    )}
                </div>
            )}

            {!carregando &&
                tecnicos.length === 0 &&
                !selecaoAtiva && (
                    <p className="estado-vazio">
                        {buscaRealizada
                            ? 'Nenhum técnico elegível encontrado para os critérios informados.'
                            : 'Faça uma busca para encontrar técnicos disponíveis.'}
                    </p>
                )}

            <section className="lista-matching">
                {tecnicos.map((tecnico, indice) => {
                    const melhorOpcao = indice === 0
                    return (
                        <article
                            className={`cartao-tecnico-matching ${
                                melhorOpcao ? 'melhor-opcao' : ''
                            }`}
                            key={tecnico.id}
                        >
                            <span className="posicao-ranking">
                                {indice + 1}º no ranking
                            </span>

                            <div className="avatar">
                                {tecnico.nomeExibicao
                                    .charAt(0)
                                    .toUpperCase()}
                            </div>

                            <div className="dados-tecnico-matching">
                                <div className="topo-cartao-matching">
                                    <div>
                                        {melhorOpcao && (
                                            <span className="selo-melhor-opcao">
                                                Melhor opção
                                            </span>
                                        )}

                                        <h2>
                                            {tecnico.nomeExibicao}
                                        </h2>

                                        <p>
                                            {
                                                tecnico
                                                    .descricaoProfissional
                                            }
                                        </p>
                                    </div>

                                    <span className="pontuacao-matching">
                                        {tecnico.pontuacao} pontos
                                    </span>
                                </div>

                                <p className="localizacao">
                                    {tecnico.cidadeAtendimento} -{' '}
                                    {tecnico.estadoAtendimento}
                                </p>

                                <div className="detalhes-matching">
                                    <span className="etiqueta">
                                        {tecnico.especialidade}
                                    </span>

                                    <span className="etiqueta">
                                        {formatarNivel(
                                            tecnico.nivelExperiencia
                                        )}
                                    </span>

                                    <span className="etiqueta">
                                        {tecnico.anosExperiencia}{' '}
                                        {tecnico.anosExperiencia === 1
                                            ? 'ano'
                                            : 'anos'}{' '}
                                        de experiência
                                    </span>

                                    {tecnico.destaques.map(
                                        (destaque, idx) => (
                                            <span
                                                key={idx}
                                                className="etiqueta destaque"
                                            >
                                                {destaque}
                                            </span>
                                        )
                                    )}
                                </div>

                                <button
                                    className="botao-principal"
                                    type="button"
                                    disabled={
                                        ocupado || selecaoAtiva
                                    }
                                    onClick={() =>
                                        escolherTecnico(tecnico)
                                    }
                                >
                                    {selecionandoId ===
                                    tecnico.id
                                        ? 'Selecionando...'
                                        : 'Escolher técnico'}
                                </button>
                            </div>
                        </article>
                    )
                })}
            </section>
        </>
    )
}
