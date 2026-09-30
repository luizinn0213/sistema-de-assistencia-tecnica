import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Toast } from '../components/Toast'
import { obterSessao } from '../services/authApi'
import {
    atualizarEquipamento,
    cadastrarEquipamento,
    excluirEquipamento,
    listarEquipamentos
} from '../services/equipamentosApi'
import type { Equipamento } from '../types/Equipamento'

export function Equipamentos() {
    const sessao = obterSessao()
    const clienteId = sessao?.clienteId

    const [equipamentos, setEquipamentos] =
        useState<Equipamento[]>([])

    const [tipo, setTipo] = useState('')
    const [marca, setMarca] = useState('')
    const [modelo, setModelo] = useState('')
    const [numeroSerie, setNumeroSerie] = useState('')

    const [editandoId, setEditandoId] =
        useState<number | null>(null)

    const [carregando, setCarregando] =
        useState(Boolean(clienteId))

    const [salvando, setSalvando] = useState(false)
    const [erro, setErro] = useState('')
    const [mensagem, setMensagem] = useState('')

    useEffect(() => {
        if (!clienteId) {
            return
        }

        let componenteAtivo = true

        listarEquipamentos(clienteId)
            .then((dados) => {
                if (componenteAtivo) {
                    setEquipamentos(dados)
                }
            })
            .catch((erroRecebido) => {
                if (!componenteAtivo) {
                    return
                }

                if (erroRecebido instanceof Error) {
                    setErro(erroRecebido.message)
                } else {
                    setErro(
                        'Não foi possível carregar os equipamentos.'
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

    async function carregarEquipamentos(
        idCliente: number
    ) {
        setCarregando(true)
        setErro('')

        try {
            const dados = await listarEquipamentos(
                idCliente
            )

            setEquipamentos(dados)
        } catch (erroRecebido) {
            tratarErro(erroRecebido)
        } finally {
            setCarregando(false)
        }
    }

    async function salvar(
        evento: FormEvent<HTMLFormElement>
    ) {
        evento.preventDefault()

        if (!clienteId) {
            setErro('Faça login como cliente.')
            return
        }

        setSalvando(true)
        setErro('')
        setMensagem('')

        const dados = {
            tipo,
            marca,
            modelo,
            numeroSerie
        }

        try {
            if (editandoId) {
                await atualizarEquipamento(
                    editandoId,
                    dados
                )

                setMensagem(
                    'Equipamento atualizado com sucesso.'
                )
            } else {
                await cadastrarEquipamento(
                    clienteId,
                    dados
                )

                setMensagem(
                    'Equipamento cadastrado com sucesso.'
                )
            }

            limparFormulario()
            await carregarEquipamentos(clienteId)
        } catch (erroRecebido) {
            tratarErro(erroRecebido)
        } finally {
            setSalvando(false)
        }
    }

    function iniciarEdicao(
        equipamento: Equipamento
    ) {
        setEditandoId(equipamento.id)
        setTipo(equipamento.tipo)
        setMarca(equipamento.marca)
        setModelo(equipamento.modelo)
        setNumeroSerie(equipamento.numeroSerie)
        setErro('')
        setMensagem('')
    }

    async function remover(
        equipamento: Equipamento
    ) {
        const confirmou = window.confirm(
            `Excluir o equipamento ${equipamento.marca} ${equipamento.modelo}?`
        )

        if (!confirmou || !clienteId) {
            return
        }

        setErro('')
        setMensagem('')

        try {
            await excluirEquipamento(equipamento.id)

            setMensagem(
                'Equipamento excluído com sucesso.'
            )

            await carregarEquipamentos(clienteId)
        } catch (erroRecebido) {
            tratarErro(erroRecebido)
        }
    }

    function limparFormulario() {
        setEditandoId(null)
        setTipo('')
        setMarca('')
        setModelo('')
        setNumeroSerie('')
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
                {erro && (
                    <Toast
                        mensagem={erro}
                        tipo="erro"
                        aoFechar={() => setErro('')}
                    />
                )}

                <div className="cabecalho-formulario">
                    <span className="subtitulo-formulario">
                        Área do cliente
                    </span>

                    <h1>Meus equipamentos</h1>

                    <p>
                        Faça login como cliente para visualizar
                        e cadastrar seus equipamentos.
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

                <h1>Meus equipamentos</h1>

                <p>
                    Cadastre os aparelhos que poderão receber
                    assistência técnica.
                </p>
            </div>

            <section className="painel-equipamentos">
                <form
                    className="formulario-equipamento"
                    onSubmit={salvar}
                >
                    <h2>
                        {editandoId
                            ? 'Editar equipamento'
                            : 'Novo equipamento'}
                    </h2>

                    <label>
                        Tipo

                        <input
                            required
                            placeholder="Ex.: Notebook"
                            value={tipo}
                            onChange={(evento) =>
                                setTipo(evento.target.value)
                            }
                        />
                    </label>

                    <label>
                        Marca

                        <input
                            required
                            placeholder="Ex.: Dell"
                            value={marca}
                            onChange={(evento) =>
                                setMarca(evento.target.value)
                            }
                        />
                    </label>

                    <label>
                        Modelo

                        <input
                            required
                            placeholder="Ex.: Inspiron 15"
                            value={modelo}
                            onChange={(evento) =>
                                setModelo(evento.target.value)
                            }
                        />
                    </label>

                    <label>
                        Número de série

                        <input
                            required
                            minLength={4}
                            value={numeroSerie}
                            onChange={(evento) =>
                                setNumeroSerie(
                                    evento.target.value
                                )
                            }
                        />
                    </label>

                    <div className="acoes-equipamento">
                        <button
                            className="botao-principal"
                            type="submit"
                            disabled={salvando}
                        >
                            {salvando
                                ? 'Salvando...'
                                : editandoId
                                  ? 'Salvar alterações'
                                  : 'Cadastrar'}
                        </button>

                        {editandoId && (
                            <button
                                className="botao-secundario"
                                type="button"
                                onClick={limparFormulario}
                            >
                                Cancelar
                            </button>
                        )}
                    </div>
                </form>

                <div className="lista-equipamentos">
                    <div className="titulo-lista-equipamentos">
                        <div>
                            <span>Seus aparelhos</span>

                            <h2>
                                Equipamentos cadastrados
                            </h2>
                        </div>

                        <strong>
                            {equipamentos.length}
                        </strong>
                    </div>

                    {carregando && (
                        <p>Carregando equipamentos...</p>
                    )}

                    {!carregando &&
                        equipamentos.length === 0 && (
                            <p className="estado-vazio">
                                Nenhum equipamento cadastrado.
                            </p>
                        )}

                    {equipamentos.map((equipamento) => (
                        <article
                            className="cartao-equipamento"
                            key={equipamento.id}
                        >
                            <div className="icone-equipamento">
                                {equipamento.tipo
                                    .charAt(0)
                                    .toUpperCase()}
                            </div>

                            <div className="dados-equipamento">
                                <span>
                                    {equipamento.tipo}
                                </span>

                                <h3>
                                    {equipamento.marca}{' '}
                                    {equipamento.modelo}
                                </h3>

                                <p>
                                    Série:{' '}
                                    {equipamento.numeroSerie}
                                </p>
                            </div>

                            <div className="botoes-equipamento">
                                <button
                                    type="button"
                                    onClick={() =>
                                        iniciarEdicao(
                                            equipamento
                                        )
                                    }
                                >
                                    Editar
                                </button>

                                <button
                                    className="acao-perigosa"
                                    type="button"
                                    onClick={() =>
                                        remover(equipamento)
                                    }
                                >
                                    Excluir
                                </button>
                            </div>
                        </article>
                    ))}
                </div>
            </section>
        </main>
    )
}