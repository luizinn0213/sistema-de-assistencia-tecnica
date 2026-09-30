import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Toast } from '../components/Toast'
import {
    encerrarSessao,
    obterSessao
} from '../services/authApi'
import {
    atualizarCliente,
    buscarCliente
} from '../services/clientesApi'

export function PerfilCliente() {
    const sessao = obterSessao()
    const clienteId = sessao?.clienteId

    const [nome, setNome] = useState('')
    const [email, setEmail] = useState('')
    const [telefone, setTelefone] = useState('')
    const [cpfCnpj, setCpfCnpj] = useState('')

    const [cep, setCep] = useState('')
    const [logradouro, setLogradouro] = useState('')
    const [numero, setNumero] = useState('')
    const [bairro, setBairro] = useState('')
    const [cidade, setCidade] = useState('')
    const [estado, setEstado] = useState('')

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

        buscarCliente(clienteId)
            .then((cliente) => {
                if (!componenteAtivo) {
                    return
                }

                setNome(cliente.nome)
                setEmail(cliente.email)
                setTelefone(cliente.telefone ?? '')
                setCpfCnpj(cliente.cpfCnpj)

                setCep(cliente.endereco?.cep ?? '')
                setLogradouro(
                    cliente.endereco?.logradouro ?? ''
                )
                setNumero(
                    cliente.endereco?.numero ?? ''
                )
                setBairro(
                    cliente.endereco?.bairro ?? ''
                )
                setCidade(
                    cliente.endereco?.cidade ?? ''
                )
                setEstado(
                    cliente.endereco?.estado ?? ''
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
                        'Não foi possível carregar o perfil.'
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

        try {
            const resultado = await atualizarCliente(
                clienteId,
                {
                    nome,
                    telefone,
                    endereco: {
                        cep,
                        logradouro,
                        numero,
                        bairro,
                        cidade,
                        estado
                    }
                }
            )

            setMensagem(resultado.mensagem)

            if (sessao) {
                localStorage.setItem(
                    'sessaoUsuario',
                    JSON.stringify({
                        ...sessao,
                        nome
                    })
                )
            }
        } catch (erroRecebido) {
            if (erroRecebido instanceof Error) {
                setErro(erroRecebido.message)
            } else {
                setErro(
                    'Não foi possível atualizar o perfil.'
                )
            }
        } finally {
            setSalvando(false)
        }
    }

    function sairDaConta() {
        const confirmou = window.confirm(
            'Deseja realmente sair da conta?'
        )

        if (!confirmou) {
            return
        }

        encerrarSessao()
        window.location.reload()
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

                    <h1>Meu perfil</h1>

                    <p>
                        Faça login para consultar e alterar
                        seus dados.
                    </p>
                </div>
            </main>
        )
    }

    if (carregando) {
        return (
            <main className="pagina-conteudo">
                <p>Carregando perfil...</p>
            </main>
        )
    }

    return (
        <main className="pagina-perfil-cliente">
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

                <h1>Meu perfil</h1>

                <p>
                    Consulte e mantenha seus dados atualizados.
                </p>
            </div>

            <form
                className="formulario-perfil-cliente"
                onSubmit={salvar}
            >
                <section className="secao-formulario">
                    <div className="titulo-secao">
                        <span>1</span>

                        <div>
                            <h2>Dados pessoais</h2>

                            <p>
                                Informações vinculadas à conta.
                            </p>
                        </div>
                    </div>

                    <div className="grade-formulario">
                        <label className="campo-largo">
                            Nome completo

                            <input
                                required
                                value={nome}
                                onChange={(evento) =>
                                    setNome(
                                        evento.target.value
                                    )
                                }
                            />
                        </label>

                        <label>
                            E-mail

                            <input
                                disabled
                                value={email}
                            />

                            <small>
                                O e-mail não pode ser alterado.
                            </small>
                        </label>

                        <label>
                            CPF ou CNPJ

                            <input
                                disabled
                                value={cpfCnpj}
                            />

                            <small>
                                O documento não pode ser alterado.
                            </small>
                        </label>

                        <label>
                            Telefone

                            <input
                                value={telefone}
                                onChange={(evento) =>
                                    setTelefone(
                                        evento.target.value
                                    )
                                }
                            />
                        </label>
                    </div>
                </section>

                <section className="secao-formulario">
                    <div className="titulo-secao">
                        <span>2</span>

                        <div>
                            <h2>Endereço</h2>

                            <p>
                                Local utilizado nos atendimentos.
                            </p>
                        </div>
                    </div>

                    <div className="grade-formulario">
                        <label>
                            CEP

                            <input
                                required
                                value={cep}
                                onChange={(evento) =>
                                    setCep(
                                        evento.target.value
                                    )
                                }
                            />
                        </label>

                        <label className="campo-largo">
                            Logradouro

                            <input
                                required
                                value={logradouro}
                                onChange={(evento) =>
                                    setLogradouro(
                                        evento.target.value
                                    )
                                }
                            />
                        </label>

                        <label>
                            Número

                            <input
                                required
                                value={numero}
                                onChange={(evento) =>
                                    setNumero(
                                        evento.target.value
                                    )
                                }
                            />
                        </label>

                        <label>
                            Bairro

                            <input
                                required
                                value={bairro}
                                onChange={(evento) =>
                                    setBairro(
                                        evento.target.value
                                    )
                                }
                            />
                        </label>

                        <label>
                            Cidade

                            <input
                                required
                                value={cidade}
                                onChange={(evento) =>
                                    setCidade(
                                        evento.target.value
                                    )
                                }
                            />
                        </label>

                        <label>
                            Estado

                            <input
                                required
                                maxLength={2}
                                value={estado}
                                onChange={(evento) =>
                                    setEstado(
                                        evento.target.value
                                            .toUpperCase()
                                    )
                                }
                            />
                        </label>
                    </div>
                </section>

                <div className="acoes-formulario acoes-perfil-cliente">
                    <button
                        className="botao-sair"
                        type="button"
                        onClick={sairDaConta}
                    >
                        Sair da conta
                    </button>

                    <button
                        className="botao-principal"
                        type="submit"
                        disabled={salvando}
                    >
                        {salvando
                            ? 'Salvando...'
                            : 'Salvar alterações'}
                    </button>
                </div>
            </form>
        </main>
    )
}