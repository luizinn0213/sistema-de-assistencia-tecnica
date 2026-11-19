import { useState } from 'react'
import type { FormEvent } from 'react'
import { Toast } from '../components/Toast'
import { obterSessao, trocarSenha } from '../services/authApi'

export function TrocarSenha() {
    const sessao = obterSessao()
    const [senhaAtual, setSenhaAtual] = useState('')
    const [novaSenha, setNovaSenha] = useState('')
    const [confirmacao, setConfirmacao] = useState('')
    const [salvando, setSalvando] = useState(false)
    const [erro, setErro] = useState('')
    const [mensagem, setMensagem] = useState('')

    async function enviar(evento: FormEvent<HTMLFormElement>) {
        evento.preventDefault()
        setErro('')
        setMensagem('')

        if (novaSenha !== confirmacao) {
            setErro('A confirmação da nova senha não confere.')
            return
        }

        setSalvando(true)
        try {
            setMensagem(await trocarSenha(senhaAtual, novaSenha))
            setSenhaAtual('')
            setNovaSenha('')
            setConfirmacao('')
        } catch (recebido) {
            setErro(
                recebido instanceof Error
                    ? recebido.message
                    : 'Não foi possível alterar a senha.'
            )
        } finally {
            setSalvando(false)
        }
    }

    if (!sessao) {
        return (
            <main className="pagina-conteudo">
                <p className="estado-vazio">Faça login para alterar sua senha.</p>
            </main>
        )
    }

    return (
        <main className="pagina-login">
            {erro && <Toast mensagem={erro} tipo="erro" aoFechar={() => setErro('')} />}
            {mensagem && <Toast mensagem={mensagem} tipo="sucesso" aoFechar={() => setMensagem('')} />}

            <section className="cartao-login">
                <div className="cabecalho-login">
                    <span className="subtitulo-formulario">Segurança da conta</span>
                    <h1>Trocar senha</h1>
                    <p>Ao concluir, as sessões anteriores serão encerradas.</p>
                </div>

                <form className="formulario-login" onSubmit={enviar}>
                    <label>
                        Senha atual
                        <input required type="password" value={senhaAtual}
                            onChange={(e) => setSenhaAtual(e.target.value)} />
                    </label>
                    <label>
                        Nova senha
                        <input required minLength={6} maxLength={64} type="password"
                            value={novaSenha} onChange={(e) => setNovaSenha(e.target.value)} />
                    </label>
                    <label>
                        Confirmar nova senha
                        <input required minLength={6} maxLength={64} type="password"
                            value={confirmacao} onChange={(e) => setConfirmacao(e.target.value)} />
                    </label>
                    <button className="botao-principal" disabled={salvando}>
                        {salvando ? 'Alterando...' : 'Alterar senha'}
                    </button>
                </form>
            </section>
        </main>
    )
}
