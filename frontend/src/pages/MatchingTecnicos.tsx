import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { listarEspecialidades } from '../services/especialidadesApi'
import {
    buscarTecnicosCompativeis,
    selecionarTecnico
} from '../services/matchingApi'
import type { Especialidade } from '../types/Especialidade'
import type {
    SelecaoTecnico,
    TecnicoCompativel
} from '../types/Matching'

export function MatchingTecnicos() {
    const [especialidades, setEspecialidades] =
        useState<Especialidade[]>([])

    const [solicitacaoId, setSolicitacaoId] = useState(1)
    const [especialidadeId, setEspecialidadeId] = useState(0)
    const [cidade, setCidade] = useState('')

    const [tecnicos, setTecnicos] =
        useState<TecnicoCompativel[]>([])

    const [carregando, setCarregando] = useState(false)
    const [selecionandoId, setSelecionandoId] =
        useState<number | null>(null)

    const [erro, setErro] = useState('')
    const [selecao, setSelecao] =
        useState<SelecaoTecnico | null>(null)

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

    async function buscarTecnicos(
        evento: FormEvent<HTMLFormElement>
    ) {
        evento.preventDefault()

        if (especialidadeId <= 0) {
            setErro('Selecione uma especialidade.')
            return
        }

        setCarregando(true)
        setErro('')
        setSelecao(null)

        try {
            const resultado =
                await buscarTecnicosCompativeis(
                    especialidadeId,
                    cidade
                )

            setTecnicos(resultado)
        } catch (erroRecebido) {
            if (erroRecebido instanceof Error) {
                setErro(erroRecebido.message)
            } else {
                setErro(
                    'Não foi possível buscar os técnicos.'
                )
            }
        } finally {
            setCarregando(false)
        }
    }

    async function escolherTecnico(
        tecnico: TecnicoCompativel
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
        } catch (erroRecebido) {
            if (erroRecebido instanceof Error) {
                setErro(erroRecebido.message)
            } else {
                setErro(
                    'Não foi possível selecionar o técnico.'
                )
            }
        } finally {
            setSelecionandoId(null)
        }
    }

    return (
        <main className="pagina-conteudo pagina-matching">
            <div className="cabecalho-formulario">
                <span className="subtitulo-formulario">
                    Escolha do profissional
                </span>

                <h1>Encontrar técnico</h1>

                <p>
                    Busque técnicos disponíveis de acordo com a
                    especialidade necessária.
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
                </div>
            )}

            {!carregando &&
                tecnicos.length === 0 &&
                !selecao && (
                    <p className="estado-vazio">
                        Faça uma busca para encontrar técnicos
                        disponíveis.
                    </p>
                )}

            <section className="lista-matching">
                {tecnicos.map((tecnico) => (
                    <article
                        className="cartao-tecnico-matching"
                        key={tecnico.id}
                    >
                        <div className="avatar">
                            {tecnico.nomeExibicao
                                .charAt(0)
                                .toUpperCase()}
                        </div>

                        <div className="dados-tecnico-matching">
                            <div className="topo-cartao-matching">
                                <div>
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
                                    {
                                        tecnico
                                            .nivelExperiencia
                                    }
                                </span>

                                <span className="etiqueta">
                                    {tecnico.anosExperiencia}{' '}
                                    anos de experiência
                                </span>

                                {tecnico.mesmaCidade && (
                                    <span className="etiqueta destaque">
                                        Atende na sua cidade
                                    </span>
                                )}
                            </div>

                            <button
                                className="botao-principal"
                                type="button"
                                disabled={
                                    selecionandoId === tecnico.id ||
                                    selecao !== null
                                }
                                onClick={() =>
                                    escolherTecnico(tecnico)
                                }
                            >
                                {selecionandoId === tecnico.id
                                    ? 'Selecionando...'
                                    : 'Escolher técnico'}
                            </button>
                        </div>
                    </article>
                ))}
            </section>
        </main>
    )
}