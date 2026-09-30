import { useState } from 'react'
import type { FormEvent } from 'react'
import { Toast } from '../components/Toast'
import { realizarLogin } from '../services/authApi'
import type { SessaoUsuario } from '../types/Sessao'

export function Login() {
    const [email, setEmail] = useState('')
    const [senha, setSenha] = useState('')
    const [usuario, setUsuario] =
        useState<SessaoUsuario | null>(null)

    const [entrando, setEntrando] = useState(false)
    const [erro, setErro] = useState('')
    const [mensagem, setMensagem] = useState('')

    async function entrar(
        evento: FormEvent<HTMLFormElement>
    ) {
        evento.preventDefault()

        setEntrando(true)
        setErro('')
        setMensagem('')

        try {
            const sessao = await realizarLogin({
                email,
                senha
            })

            setUsuario(sessao)
            setSenha('')
            setMensagem('Login realizado com sucesso.')
        } catch (erroRecebido) {
            if (erroRecebido instanceof Error) {
                setErro(erroRecebido.message)
            } else {
                setErro(
                    'Não foi possível realizar o login.'
                )
            }
        } finally {
            setEntrando(false)
        }
    }

    if (usuario) {
        return (
            <>
                {mensagem && (
                    <Toast
                        mensagem={mensagem}
                        tipo="sucesso"
                        aoFechar={() =>
                            setMensagem('')
                        }
                    />
                )}

                <main className="pagina-login">
                    <section className="cartao-login sucesso-login">
                        <span className="subtitulo-formulario">
                            Acesso realizado
                        </span>

                        <h1>Olá, {usuario.nome}!</h1>

                        <p>
                            Você entrou como {usuario.tipo}.
                        </p>

                        {usuario.clienteId && (
                            <p>
                                Perfil de cliente número{' '}
                                <strong>
                                    {usuario.clienteId}
                                </strong>
                            </p>
                        )}
                    </section>
                </main>
            </>
        )
    }

    return (
        <main className="pagina-login">
            {erro && (
                <Toast
                    mensagem={erro}
                    tipo="erro"
                    aoFechar={() => setErro('')}
                />
            )}

            <section className="cartao-login">
                <div className="cabecalho-login">
                    <span className="subtitulo-formulario">
                        Área do usuário
                    </span>

                    <h1>Entrar no sistema</h1>

                    <p>
                        Acesse sua conta para gerenciar seus
                        equipamentos e solicitações.
                    </p>
                </div>

                <form
                    className="formulario-login"
                    onSubmit={entrar}
                >
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
                            value={senha}
                            onChange={(evento) =>
                                setSenha(
                                    evento.target.value
                                )
                            }
                        />
                    </label>

                    <button
                        className="botao-principal"
                        type="submit"
                        disabled={entrando}
                    >
                        {entrando
                            ? 'Entrando...'
                            : 'Entrar'}
                    </button>
                </form>
            </section>
        </main>
    )
}