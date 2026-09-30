import { useState } from 'react'
import type { FormEvent } from 'react'
import { Toast } from '../components/Toast'
import { cadastrarCliente } from '../services/clientesApi'

export function CadastroCliente() {
    const [nome, setNome] = useState('')
    const [email, setEmail] = useState('')
    const [senha, setSenha] = useState('')
    const [telefone, setTelefone] = useState('')
    const [cpfCnpj, setCpfCnpj] = useState('')

    const [cep, setCep] = useState('')
    const [logradouro, setLogradouro] = useState('')
    const [numero, setNumero] = useState('')
    const [bairro, setBairro] = useState('')
    const [cidade, setCidade] = useState('')
    const [estado, setEstado] = useState('')

    const [salvando, setSalvando] = useState(false)
    const [erro, setErro] = useState('')
    const [mensagem, setMensagem] = useState('')

    async function cadastrar(
        evento: FormEvent<HTMLFormElement>
    ) {
        evento.preventDefault()

        setSalvando(true)
        setErro('')
        setMensagem('')

        try {
            const cliente = await cadastrarCliente({
                nome,
                email,
                senha,
                telefone,
                cpfCnpj,
                endereco: {
                    cep,
                    logradouro,
                    numero,
                    bairro,
                    cidade,
                    estado
                }
            })

            setMensagem(
                `Cliente ${cliente.nome} cadastrado com sucesso.`
            )

            setNome('')
            setEmail('')
            setSenha('')
            setTelefone('')
            setCpfCnpj('')
            setCep('')
            setLogradouro('')
            setNumero('')
            setBairro('')
            setCidade('')
            setEstado('')
        } catch (erroRecebido) {
            if (erroRecebido instanceof Error) {
                setErro(erroRecebido.message)
            } else {
                setErro(
                    'Não foi possível cadastrar o cliente.'
                )
            }
        } finally {
            setSalvando(false)
        }
    }

    return (
        <main className="pagina-cadastro-cliente">
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
                    Crie sua conta
                </span>

                <h1>Cadastro de cliente</h1>

                <p>
                    Informe seus dados para solicitar serviços
                    de assistência técnica.
                </p>
            </div>

            <form
                className="formulario-cliente"
                onSubmit={cadastrar}
            >
                <section className="secao-formulario">
                    <div className="titulo-secao">
                        <span>1</span>

                        <div>
                            <h2>Dados pessoais</h2>
                            <p>
                                Informações utilizadas para
                                identificar o cliente.
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
                                required
                                type="email"
                                value={email}
                                onChange={(evento) =>
                                    setEmail(
                                        evento.target.value
                                    )
                                }
                            />
                        </label>

                        <label>
                            Senha

                            <input
                                required
                                type="password"
                                minLength={6}
                                value={senha}
                                onChange={(evento) =>
                                    setSenha(
                                        evento.target.value
                                    )
                                }
                            />
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

                        <label>
                            CPF ou CNPJ

                            <input
                                required
                                value={cpfCnpj}
                                onChange={(evento) =>
                                    setCpfCnpj(
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
                                Local utilizado para os
                                atendimentos.
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
                                placeholder="RJ"
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

                <div className="acoes-formulario">
                    <button
                        className="botao-principal"
                        type="submit"
                        disabled={salvando}
                    >
                        {salvando
                            ? 'Cadastrando...'
                            : 'Criar conta'}
                    </button>
                </div>
            </form>
        </main>
    )
}