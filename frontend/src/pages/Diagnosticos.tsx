import {
    useState,
    type FormEvent
} from 'react'
import { Toast } from '../components/Toast'
import {
    atualizarDiagnostico,
    concluirDiagnostico,
    criarDiagnostico,
    obterDiagnosticoPorSolicitacao
} from '../services/diagnosticosApi'
import type {
    Diagnostico
} from '../types/DiagnosticoOrcamento'

type TipoToast = 'sucesso' | 'erro'

export function Diagnosticos() {
    const [solicitacaoId, setSolicitacaoId] =
        useState('')

    const [diagnostico, setDiagnostico] =
        useState<Diagnostico | null>(null)

    const [descricaoDefeito, setDescricaoDefeito] =
        useState('')

    const [
        solucaoRecomendada,
        setSolucaoRecomendada
    ] = useState('')

    const [carregando, setCarregando] =
        useState(false)

    const [mensagem, setMensagem] =
        useState('')

    const [tipoMensagem, setTipoMensagem] =
        useState<TipoToast>('sucesso')

    const concluido =
        diagnostico?.status === 'Concluido'

    function mostrarMensagem(
        texto: string,
        tipo: TipoToast
    ) {
        setMensagem(texto)
        setTipoMensagem(tipo)
    }

    function preencherFormulario(
        dados: Diagnostico
    ) {
        setDiagnostico(dados)
        setDescricaoDefeito(
            dados.descricaoDefeito
        )
        setSolucaoRecomendada(
            dados.solucaoRecomendada
        )
    }

    function obterSolicitacaoId(): number | null {
        const id = Number(solicitacaoId)

        if (!Number.isInteger(id) || id <= 0) {
            mostrarMensagem(
                'Informe um ID de solicitação válido.',
                'erro'
            )

            return null
        }

        return id
    }

    async function buscarDiagnostico(
        evento: FormEvent
    ) {
        evento.preventDefault()

        const id = obterSolicitacaoId()

        if (id === null) {
            return
        }

        setCarregando(true)

        try {
            const dados =
                await obterDiagnosticoPorSolicitacao(
                    id
                )

            preencherFormulario(dados)

            mostrarMensagem(
                'Diagnóstico encontrado.',
                'sucesso'
            )
        } catch (erro) {
            setDiagnostico(null)
            setDescricaoDefeito('')
            setSolucaoRecomendada('')

            mostrarMensagem(
                erro instanceof Error
                    ? erro.message
                    : 'Não foi possível buscar o diagnóstico.',
                'erro'
            )
        } finally {
            setCarregando(false)
        }
    }

    async function criar(
        evento: FormEvent
    ) {
        evento.preventDefault()

        const id = obterSolicitacaoId()

        if (id === null) {
            return
        }

        if (!descricaoDefeito.trim()) {
            mostrarMensagem(
                'Descreva o defeito encontrado.',
                'erro'
            )

            return
        }

        setCarregando(true)

        try {
            const dados = await criarDiagnostico({
                solicitacaoId: id,
                descricaoDefeito:
                    descricaoDefeito.trim()
            })

            preencherFormulario(dados)

            mostrarMensagem(
                'Diagnóstico criado com sucesso.',
                'sucesso'
            )
        } catch (erro) {
            mostrarMensagem(
                erro instanceof Error
                    ? erro.message
                    : 'Não foi possível criar o diagnóstico.',
                'erro'
            )
        } finally {
            setCarregando(false)
        }
    }

    async function salvar(
        evento: FormEvent
    ) {
        evento.preventDefault()

        if (!diagnostico) {
            mostrarMensagem(
                'Crie ou busque um diagnóstico primeiro.',
                'erro'
            )

            return
        }

        setCarregando(true)

        try {
            const dados =
                await atualizarDiagnostico(
                    diagnostico.id,
                    {
                        descricaoDefeito:
                            descricaoDefeito.trim(),
                        solucaoRecomendada:
                            solucaoRecomendada.trim()
                    }
                )

            preencherFormulario(dados)

            mostrarMensagem(
                'Diagnóstico atualizado.',
                'sucesso'
            )
        } catch (erro) {
            mostrarMensagem(
                erro instanceof Error
                    ? erro.message
                    : 'Não foi possível atualizar.',
                'erro'
            )
        } finally {
            setCarregando(false)
        }
    }

    async function concluir() {
        if (!diagnostico) {
            return
        }

        setCarregando(true)

        try {
            const dados =
                await concluirDiagnostico(
                    diagnostico.id
                )

            preencherFormulario(dados)

            mostrarMensagem(
                'Diagnóstico concluído.',
                'sucesso'
            )
        } catch (erro) {
            mostrarMensagem(
                erro instanceof Error
                    ? erro.message
                    : 'Não foi possível concluir.',
                'erro'
            )
        } finally {
            setCarregando(false)
        }
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
                    ATENDIMENTO TÉCNICO
                </span>

                <h1>Diagnóstico</h1>

                <p>
                    Registre o defeito encontrado e a
                    solução recomendada.
                </p>
            </div>

            <section className="painel-equipamentos">
                <form
                    className="formulario-equipamento"
                    onSubmit={buscarDiagnostico}
                >
                    <label>
                        ID da solicitação
                        <input
                            type="number"
                            min="1"
                            value={solicitacaoId}
                            onChange={(evento) =>
                                setSolicitacaoId(
                                    evento.target.value
                                )
                            }
                            placeholder="Ex.: 1"
                        />
                    </label>

                    <button
                        className="botao-secundario"
                        type="submit"
                        disabled={carregando}
                    >
                        {carregando
                            ? 'Buscando...'
                            : 'Buscar diagnóstico'}
                    </button>
                </form>

                <form
                    className="formulario-equipamento"
                    onSubmit={
                        diagnostico ? salvar : criar
                    }
                >
                    {diagnostico && (
                        <div>
                            <p>
                                <strong>Diagnóstico:</strong>{' '}
                                #{diagnostico.id}
                            </p>

                            <p>
                                <strong>Solicitação:</strong>{' '}
                                {diagnostico.solicitacaoNumero ||
                                    `#${diagnostico.solicitacaoId}`}
                            </p>

                            <p>
                                <strong>Status:</strong>{' '}
                                {diagnostico.status}
                            </p>
                        </div>
                    )}

                    <label>
                        Defeito encontrado
                        <textarea
                            rows={5}
                            maxLength={2000}
                            value={descricaoDefeito}
                            disabled={concluido}
                            onChange={(evento) =>
                                setDescricaoDefeito(
                                    evento.target.value
                                )
                            }
                            placeholder="Descreva o defeito identificado"
                        />
                    </label>

                    <label>
                        Solução recomendada
                        <textarea
                            rows={5}
                            maxLength={2000}
                            value={solucaoRecomendada}
                            disabled={
                                concluido || !diagnostico
                            }
                            onChange={(evento) =>
                                setSolucaoRecomendada(
                                    evento.target.value
                                )
                            }
                            placeholder={
                                diagnostico
                                    ? 'Informe a solução recomendada'
                                    : 'Crie o diagnóstico primeiro'
                            }
                        />
                    </label>

                    {!concluido && (
                        <div className="acoes-equipamento">
                            <button
                                className="botao-principal"
                                type="submit"
                                disabled={carregando}
                            >
                                {diagnostico
                                    ? 'Salvar alterações'
                                    : 'Criar diagnóstico'}
                            </button>

                            {diagnostico && (
                                <button
                                    className="botao-secundario"
                                    type="button"
                                    disabled={carregando}
                                    onClick={concluir}
                                >
                                    Concluir diagnóstico
                                </button>
                            )}
                        </div>
                    )}

                    {concluido && (
                        <p className="mensagem sucesso">
                            Diagnóstico concluído. Já é
                            possível criar o orçamento.
                        </p>
                    )}
                </form>
            </section>
        </main>
    )
}   