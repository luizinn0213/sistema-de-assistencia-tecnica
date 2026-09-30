import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Toast } from '../components/Toast'
import { obterSessao } from '../services/authApi'
import { listarEquipamentos } from '../services/equipamentosApi'
import { listarEspecialidades } from '../services/especialidadesApi'
import { abrirSolicitacao } from '../services/solicitacoesApi'
import type { Equipamento } from '../types/Equipamento'
import type { Especialidade } from '../types/Especialidade'
import type { Solicitacao } from '../types/Solicitacao'

const TAMANHO_MAXIMO_DESCRICAO = 1000

export function NovaSolicitacao() {
    const sessao = obterSessao()
    const clienteId = sessao?.clienteId

    const [equipamentos, setEquipamentos] =
        useState<Equipamento[]>([])

    const [especialidades, setEspecialidades] =
        useState<Especialidade[]>([])

    const [equipamentoId, setEquipamentoId] = useState('')
    const [especialidadeId, setEspecialidadeId] = useState('')
    const [descricao, setDescricao] = useState('')

    const [solicitacaoCriada, setSolicitacaoCriada] =
        useState<Solicitacao | null>(null)

    const [carregando, setCarregando] =
        useState(Boolean(clienteId))

    const [enviando, setEnviando] = useState(false)
    const [erro, setErro] = useState('')
    const [mensagem, setMensagem] = useState('')

    useEffect(() => {
        if (!clienteId) {
            return
        }

        let componenteAtivo = true

        Promise.all([
            listarEquipamentos(clienteId),
            listarEspecialidades()
        ])
            .then(([dadosEquipamentos, dadosEspecialidades]) => {
                if (!componenteAtivo) {
                    return
                }

                setEquipamentos(dadosEquipamentos)

                // Especialidades inativas não podem ser usadas.
                setEspecialidades(
                    dadosEspecialidades.filter(
                        (especialidade) => especialidade.ativa
                    )
                )
            })
            .catch((erroRecebido) => {
                if (!componenteAtivo) {
                    return
                }

                if (erroRecebido instanceof Error) {
                    setErro(erroRecebido.message)
                } else {
                    setErro(
                        'Não foi possível carregar os dados.'
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

    async function enviar(
        evento: FormEvent<HTMLFormElement>
    ) {
        evento.preventDefault()

        if (!clienteId) {
            setErro('Faça login como cliente.')
            return
        }

        if (!descricao.trim()) {
            setErro('Descreva o problema do equipamento.')
            return
        }

        setEnviando(true)
        setErro('')
        setMensagem('')

        try {
            const solicitacao = await abrirSolicitacao(
                clienteId,
                {
                    equipamentoId: Number(equipamentoId),
                    especialidadeId: Number(especialidadeId),
                    descricaoProblema: descricao
                }
            )

            setSolicitacaoCriada(solicitacao)
            setMensagem(
                `Solicitação ${solicitacao.numero} aberta com sucesso.`
            )
            limparFormulario()
        } catch (erroRecebido) {
            tratarErro(erroRecebido)
        } finally {
            setEnviando(false)
        }
    }

    function limparFormulario() {
        setEquipamentoId('')
        setEspecialidadeId('')
        setDescricao('')
    }

    function tratarErro(erroRecebido: unknown) {
        if (erroRecebido instanceof Error) {
            setErro(erroRecebido.message)
        } else {
            setErro(
                'Não foi possível concluir a operação.'
            )
        }
    }

    if (!clienteId) {
        return (
            <main className="pagina-conteudo">
                <div className="cabecalho-formulario">
                    <span className="subtitulo-formulario">
                        Área do cliente
                    </span>

                    <h1>Nova solicitação</h1>

                    <p>
                        Faça login como cliente para abrir
                        uma solicitação de assistência.
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

            {mensagem && (
                <Toast
                    mensagem={mensagem}
                    tipo="sucesso"
                    aoFechar={() => setMensagem('')}
                />
            )}

            <div className="cabecalho-formulario">
                <span className="subtitulo-formulario">
                    Área do cliente
                </span>

                <h1>Nova solicitação</h1>

                <p>
                    Informe o equipamento, a especialidade
                    necessária e descreva o problema.
                </p>
            </div>

            <section className="painel-equipamentos">
                <form
                    className="formulario-equipamento formulario-solicitacao"
                    onSubmit={enviar}
                >
                    <h2>Dados da solicitação</h2>

                    {carregando && <p>Carregando dados...</p>}

                    {!carregando &&
                        equipamentos.length === 0 && (
                            <p className="estado-vazio">
                                Cadastre um equipamento em
                                "Meus equipamentos" antes de
                                abrir uma solicitação.
                            </p>
                        )}

                    <label>
                        Equipamento

                        <select
                            required
                            value={equipamentoId}
                            onChange={(evento) =>
                                setEquipamentoId(
                                    evento.target.value
                                )
                            }
                        >
                            <option value="">
                                Selecione o equipamento
                            </option>

                            {equipamentos.map((equipamento) => (
                                <option
                                    key={equipamento.id}
                                    value={equipamento.id}
                                >
                                    {equipamento.tipo} -{' '}
                                    {equipamento.marca}{' '}
                                    {equipamento.modelo}
                                </option>
                            ))}
                        </select>
                    </label>

                    <label>
                        Especialidade

                        <select
                            required
                            value={especialidadeId}
                            onChange={(evento) =>
                                setEspecialidadeId(
                                    evento.target.value
                                )
                            }
                        >
                            <option value="">
                                Selecione a especialidade
                            </option>

                            {especialidades.map((especialidade) => (
                                <option
                                    key={especialidade.id}
                                    value={especialidade.id}
                                >
                                    {especialidade.nome}
                                </option>
                            ))}
                        </select>
                    </label>

                    <label>
                        Descrição do problema

                        <textarea
                            required
                            rows={6}
                            maxLength={TAMANHO_MAXIMO_DESCRICAO}
                            placeholder="Ex.: O notebook não liga depois de uma queda."
                            value={descricao}
                            onChange={(evento) =>
                                setDescricao(evento.target.value)
                            }
                        />

                        <small>
                            {descricao.length}/
                            {TAMANHO_MAXIMO_DESCRICAO} caracteres
                        </small>
                    </label>

                    <button
                        className="botao-principal"
                        type="submit"
                        disabled={
                            enviando ||
                            carregando ||
                            equipamentos.length === 0
                        }
                    >
                        {enviando
                            ? 'Enviando...'
                            : 'Abrir solicitação'}
                    </button>
                </form>

                <div className="lista-equipamentos">
                    {!solicitacaoCriada && (
                        <p className="estado-vazio">
                            Após o envio, sua solicitação
                            recebe um número e fica disponível
                            para a busca de técnicos.
                        </p>
                    )}

                    {solicitacaoCriada && (
                        <article className="confirmacao-solicitacao">
                            <span className="subtitulo-formulario">
                                Solicitação aberta
                            </span>

                            <h2>{solicitacaoCriada.numero}</h2>

                            <dl>
                                <dt>Status</dt>
                                <dd>
                                    <span className="status-solicitacao">
                                        {solicitacaoCriada.status}
                                    </span>
                                </dd>

                                <dt>Data</dt>
                                <dd>
                                    {new Date(
                                        solicitacaoCriada.dataCriacao
                                    ).toLocaleString('pt-BR')}
                                </dd>

                                <dt>Equipamento</dt>
                                <dd>
                                    {solicitacaoCriada.equipamento.tipo}{' '}
                                    {solicitacaoCriada.equipamento.marca}{' '}
                                    {solicitacaoCriada.equipamento.modelo}
                                </dd>

                                <dt>Especialidade</dt>
                                <dd>
                                    {solicitacaoCriada.especialidade.nome}
                                </dd>

                                <dt>Problema</dt>
                                <dd>
                                    {solicitacaoCriada.descricaoProblema}
                                </dd>
                            </dl>
                        </article>
                    )}
                </div>
            </section>
        </main>
    )
}
