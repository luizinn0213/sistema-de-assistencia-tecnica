import { useState } from 'react'
import type { FormEvent } from 'react'
import { cadastrarTecnico } from '../services/tecnicosApi'
import type { CriarTecnico } from '../types/Tecnico'

const dadosIniciais: CriarTecnico = {
  usuarioId: 0,
  nomeExibicao: '',
  descricaoProfissional: '',
  cidadeAtendimento: '',
  estadoAtendimento: ''
}

export function CadastroTecnico() {
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
          Informe os dados que serão exibidos aos clientes.
        </p>
      </div>

      <form
        className="formulario"
        onSubmit={enviarFormulario}
      >
        <label>
          ID do usuário
          <input
            type="number"
            min="1"
            required
            value={dados.usuarioId || ''}
            onChange={(evento) =>
              setDados({
                ...dados,
                usuarioId: Number(evento.target.value)
              })
            }
          />
          <small>
            Temporário até a integração com o login.
          </small>
        </label>

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
          <p className="mensagem sucesso">{mensagem}</p>
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