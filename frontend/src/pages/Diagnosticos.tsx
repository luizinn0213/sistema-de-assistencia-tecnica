import {
    useEffect,
    useState,
    type FormEvent
} from 'react'
import { SeletorAtendimento } from '../components/SeletorAtendimento'
import type { Atendimento } from '../types/Atendimento'
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

export function Diagnosticos({ idInicial, aoOrcar }: { idInicial?: number; aoOrcar: (id: number) => void }) {
    return (
        <main className="pagina-equipamentos">
            <div className="cabecalho-formulario">
                <h1>Diagnóstico</h1>
                <p>Registre o defeito encontrado e a solução recomendada.</p>
            </div>
            <SeletorAtendimento etapa="diagnostico" idInicial={idInicial}>
                {(atendimento) => (
                    <DiagnosticoAtendimento key={atendimento.id} atendimento={atendimento} aoOrcar={() => aoOrcar(atendimento.id)} />
                )}
            </SeletorAtendimento>
        </main>
    )
}

function DiagnosticoAtendimento({ atendimento, aoOrcar }: { atendimento: Atendimento; aoOrcar: () => void }) {
    const { id: solicitacaoId, diagnosticoId } = atendimento

    const [diagnostico, setDiagnostico] =
        useState<Diagnostico | null>(null)

    const [descricaoDefeito, setDescricaoDefeito] =
        useState('')

    const [
        solucaoRecomendada,
        setSolucaoRecomendada
    ] = useState('')

    const [carregando, setCarregando] =
        useState(Boolean(diagnosticoId))

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

    useEffect(() => {
        if (!diagnosticoId) return
        let ativo = true

        obterDiagnosticoPorSolicitacao(solicitacaoId).then(dados => {
            if (ativo) preencherFormulario(dados)
        }).catch((erro: unknown) => {
            if (ativo) mostrarMensagem(
                erro instanceof Error ? erro.message : 'Não foi possível carregar o diagnóstico.',
                'erro'
            )
        }).finally(() => {
            if (ativo) setCarregando(false)
        })

        return () => { ativo = false }
    }, [diagnosticoId, solicitacaoId])

    async function criar(
        evento: FormEvent
    ) {
        evento.preventDefault()

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
                solicitacaoId,
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
        <>
            {mensagem && (
                <Toast
                    mensagem={mensagem}
                    tipo={tipoMensagem}
                    aoFechar={() => setMensagem('')}
                />
            )}

            <section className="painel-equipamentos">
                {diagnosticoId && !diagnostico && carregando && <p>Carregando diagnóstico...</p>}
                {(!diagnosticoId || diagnostico) && (
                    <form
                        className="formulario-equipamento"
                        onSubmit={
                            diagnostico ? salvar : criar
                        }
                    >
                        {diagnostico && (
                            <div>
                                <p>
                                    <strong>Solicitação:</strong>{' '}
                                    {atendimento.numero}
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
                            <>
                                <p className="mensagem sucesso">
                                    Diagnóstico concluído. Já é possível criar o orçamento.
                                </p>
                                <button type="button" className="botao-principal" onClick={aoOrcar}>
                                    Ir para orçamento
                                </button>
                            </>
                        )}
                    </form>
                )}
            </section>
        </>
    )
}   