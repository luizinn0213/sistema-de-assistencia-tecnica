import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Toast } from '../components/Toast'
import { listarEspecialidades } from '../services/especialidadesApi'
import {
    buscarRankingTecnicos,
    selecionarTecnico
} from '../services/matchingApi'
import type { Especialidade } from '../types/Especialidade'
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

export function MatchingTecnicos() {
    const [especialidades, setEspecialidades] =
        useState<Especialidade[]>([])

    const [solicitacaoId, setSolicitacaoId] = useState(1)
    const [especialidadeId, setEspecialidadeId] = useState(0)
    const [cidade, setCidade] = useState('')

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

    useEffect(() => {
        listarEspecialidades()
            .then((dados) => {
                const especialidadesAtivas = dados.filter(
                    (especialidade) => especialidade.ativa
                )

                setEspecialidades(especialidadesAtivas)

                if (especialidadesAtivas.length > 0) {
                    setEspecialidadeId(
                        especialidadesAtivas[0].id
                    )
                }
            })
            .catch(() => {
                setErro(
                    'Não foi possível carregar as especialidades.'
                )
            })
    }, [])

    async function executarBusca() {
        if (especialidadeId <= 0) {
            setErro('Selecione uma especialidade.')
            return
        }

        setCarregando(true)
        setErro('')
        setSelecao(null)

        try {
            const resultado =
                await buscarRankingTecnicos(
                    especialidadeId,
                    cidade,
                    solicitacaoId > 0 ? solicitacaoId : undefined
                )

            setTecnicos(resultado)
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
        if (solicitacaoId <= 0) {
            setErro(
                'Informe um número de solicitação válido.'
            )
            return
        }

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
        <main className="pagina-conteudo pagina-matching">
            {toast && (
                <Toast
                    mensagem={toast.mensagem}
                    tipo={toast.tipo}
                    aoFechar={() => setToast(null)}
                />
            )}

            <div className="cabecalho-formulario">
                <span className="subtitulo-formulario">
                    Escolha do profissional
                </span>

                <h1>Encontrar técnico</h1>

                <p>
                    Busque técnicos disponíveis ordenados pelo
                    ranking de compatibilidade. Técnicos que já
                    recusaram esta solicitação são filtrados
                    automaticamente.
                </p>
            </div>

            <form
                className="formulario-matching"
                onSubmit={buscarTecnicos}
            >
                <label>
                    Número da solicitação

                    <input
                        type="number"
                        min="1"
                        value={solicitacaoId}
                        onChange={(evento) =>
                            setSolicitacaoId(
                                Number(evento.target.value)
                            )
                        }
                    />
                </label>

                <label>
                    Especialidade

                    <select
                        value={especialidadeId}
                        onChange={(evento) =>
                            setEspecialidadeId(
                                Number(evento.target.value)
                            )
                        }
                    >
                        {especialidades.map(
                            (especialidade) => (
                                <option
                                    key={especialidade.id}
                                    value={especialidade.id}
                                >
                                    {especialidade.nome}
                                </option>
                            )
                        )}
                    </select>
                </label>

                <label>
                    Cidade do atendimento

                    <input
                        type="text"
                        value={cidade}
                        placeholder="Ex.: Teresópolis"
                        onChange={(evento) =>
                            setCidade(evento.target.value)
                        }
                    />
                </label>

                <button
                    className="botao-principal"
                    type="submit"
                    disabled={carregando}
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
                    <strong>Técnico selecionado!</strong>

                    <span>
                        {selecao.tecnico} ·{' '}
                        {selecao.especialidade}
                    </span>

                    <span>
                        Status: {selecao.status}
                    </span>

                    <button
                        type="button"
                        className="botao-secundario"
                        onClick={executarBusca}
                    >
                        Buscar novamente (caso haja recusa)
                    </button>
                </div>
            )}

            {!carregando &&
                tecnicos.length === 0 &&
                !selecao && (
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
                                        selecionandoId ===
                                            tecnico.id ||
                                        selecao !== null
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
        </main>
    )
}
