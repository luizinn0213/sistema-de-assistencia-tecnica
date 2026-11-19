import { useState } from 'react'
import type { FormEvent } from 'react'
import { cadastrarTecnico } from '../services/tecnicosApi'
import type { CriarTecnico } from '../types/Tecnico'

const dadosIniciais: CriarTecnico = {
  email: '',
  senha: '',
  nomeExibicao: '',
  descricaoProfissional: '',
  cidadeAtendimento: '',
  estadoAtendimento: ''
}

export function CadastroTecnico({ aoEntrar }: { aoEntrar: () => void }) {
  const [dados, setDados] =
    useState<CriarTecnico>(dadosIniciais)

  const [enviando, setEnviando] = useState(false)
  const [mensagem, setMensagem] = useState('')
  const [erro, setErro] = useState('')

  async function enviarFormulario(
    evento: FormEvent<HTMLFormElement>
  ) {
    evento.preventDefault()

    setEnviando(true)
    setMensagem('')
    setErro('')

    try {
      const tecnico = await cadastrarTecnico(dados)

      setMensagem(
        `Técnico ${tecnico.nomeExibicao} cadastrado com sucesso.`
      )

      setDados(dadosIniciais)
    } catch (erroRecebido) {
      if (erroRecebido instanceof Error) {
        setErro(erroRecebido.message)
      } else {
        setErro('Não foi possível cadastrar o técnico.')
      }
    } finally {
      setEnviando(false)
    }
  }

  return (
    <main className="pagina-conteudo">
      <div className="cabecalho-formulario">
        <span className="subtitulo-formulario">
          Perfil profissional
        </span>

        <h1>Cadastrar técnico</h1>

        <p>
          Crie seu acesso e preencha os dados do seu perfil profissional.
        </p>
      </div>

      <form
        className="formulario"
        onSubmit={enviarFormulario}
      >
        <label>
          Nome de exibição
          <input
            type="text"
            required
            value={dados.nomeExibicao}
            onChange={(evento) =>
              setDados({
                ...dados,
                nomeExibicao: evento.target.value
              })
            }
          />
        </label>

        <label>
          E-mail
          <input
            type="email"
            autoComplete="email"
            required
            value={dados.email}
            onChange={(evento) =>
              setDados({ ...dados, email: evento.target.value })
            }
          />
        </label>

        <label>
          Senha
          <input
            type="password"
            autoComplete="new-password"
            minLength={6}
            required
            value={dados.senha}
            onChange={(evento) =>
              setDados({ ...dados, senha: evento.target.value })
            }
          />
        </label>

        <label>
          Descrição profissional
          <textarea
            rows={4}
            required
            value={dados.descricaoProfissional}
            onChange={(evento) =>
              setDados({
                ...dados,
                descricaoProfissional:
                  evento.target.value
              })
            }
          />
        </label>

        <div className="linha-formulario">
          <label>
            Cidade de atendimento
            <input
              type="text"
              required
              value={dados.cidadeAtendimento}
              onChange={(evento) =>
                setDados({
                  ...dados,
                  cidadeAtendimento:
                    evento.target.value
                })
              }
            />
          </label>

          <label>
            Estado
            <input
              type="text"
              required
              minLength={2}
              maxLength={2}
              placeholder="RJ"
              value={dados.estadoAtendimento}
              onChange={(evento) =>
                setDados({
                  ...dados,
                  estadoAtendimento:
                    evento.target.value.toUpperCase()
                })
              }
            />
          </label>
        </div>

        {mensagem && (
          <>
            <p className="mensagem sucesso">{mensagem}</p>
            <button className="botao-secundario" type="button" onClick={aoEntrar}>
              Entrar na conta
            </button>
          </>
        )}

        {erro && (
          <p className="mensagem erro">{erro}</p>
        )}

        <button
          className="botao-principal"
          type="submit"
          disabled={enviando}
        >
          {enviando
            ? 'Cadastrando...'
            : 'Cadastrar técnico'}
        </button>
      </form>
    </main>
  )
}