import { obterSessao } from '../services/authApi'

export type Pagina =
    | 'tecnicos'
    | 'matching'
    | 'solicitacoes-tecnico'
    | 'cadastro-tecnico'
    | 'perfil-tecnico'
    | 'especialidades'
    | 'cadastro-cliente'
    | 'equipamentos'
    | 'login'
    | 'perfil-cliente'
    | 'nova-solicitacao'
    | 'minhas-solicitacoes'

interface HeaderProps {
    paginaAtual: Pagina
    aoNavegar: (pagina: Pagina) => void
}

export function Header({
    paginaAtual,
    aoNavegar
}: HeaderProps) {
    const sessao = obterSessao()

    const iniciais = sessao
        ? sessao.nome
              .split(' ')
              .filter(Boolean)
              .slice(0, 2)
              .map((parte) => parte.charAt(0))
              .join('')
              .toUpperCase()
        : 'EN'

    function abrirPerfil() {
        const sessaoAtual = obterSessao()

        if (sessaoAtual?.clienteId) {
            aoNavegar('perfil-cliente')
        } else {
            aoNavegar('login')
        }
    }

    return (
        <header className="barra-superior">
            <button
                className="logo"
                type="button"
                onClick={() => aoNavegar('tecnicos')}
            >
                <span>NG</span>
                OS NARGGETS
            </button>

            <nav className="menu-principal">
                <button
                    className={
                        paginaAtual === 'tecnicos'
                            ? 'ativo'
                            : ''
                    }
                    type="button"
                    onClick={() => aoNavegar('tecnicos')}
                >
                    Buscar técnicos
                </button>

                <button
                    className={
                        paginaAtual === 'matching'
                            ? 'ativo'
                            : ''
                    }
                    type="button"
                    onClick={() => aoNavegar('matching')}
                >
                    Escolher técnico
                </button>

                <button
                    className={
                        paginaAtual === 'solicitacoes-tecnico'
                            ? 'ativo'
                            : ''
                    }
                    type="button"
                    onClick={() =>
                        aoNavegar('solicitacoes-tecnico')
                    }
                >
                    Solicitações
                </button>

                <button
                    className={
                        paginaAtual === 'cadastro-tecnico'
                            ? 'ativo'
                            : ''
                    }
                    type="button"
                    onClick={() =>
                        aoNavegar('cadastro-tecnico')
                    }
                >
                    Cadastrar técnico
                </button>

                <button
                    className={
                        paginaAtual === 'perfil-tecnico'
                            ? 'ativo'
                            : ''
                    }
                    type="button"
                    onClick={() =>
                        aoNavegar('perfil-tecnico')
                    }
                >
                    Perfil técnico
                </button>

                <button
                    className={
                        paginaAtual === 'especialidades'
                            ? 'ativo'
                            : ''
                    }
                    type="button"
                    onClick={() =>
                        aoNavegar('especialidades')
                    }
                >
                    Especialidades
                </button>

                <button
                    className={
                        paginaAtual === 'cadastro-cliente'
                            ? 'ativo'
                            : ''
                    }
                    type="button"
                    onClick={() =>
                        aoNavegar('cadastro-cliente')
                    }
                >
                    Criar conta
                </button>

                <button
                    className={
                        paginaAtual === 'equipamentos'
                            ? 'ativo'
                            : ''
                    }
                    type="button"
                    onClick={() =>
                        aoNavegar('equipamentos')
                    }
                >
                    Meus equipamentos
                </button>

                <button
                    className={
                        paginaAtual === 'nova-solicitacao'
                            ? 'ativo'
                            : ''
                    }
                    type="button"
                    onClick={() =>
                        aoNavegar('nova-solicitacao')
                    }
                >
                    Nova solicitação
                </button>

                <button
                    className={
                        paginaAtual === 'minhas-solicitacoes'
                            ? 'ativo'
                            : ''
                    }
                    type="button"
                    onClick={() =>
                        aoNavegar('minhas-solicitacoes')
                    }
                >
                    Minhas solicitações
                </button>
            </nav>

            <button
                className={
                    paginaAtual === 'login' ||
                    paginaAtual === 'perfil-cliente'
                        ? 'botao-perfil ativo'
                        : 'botao-perfil'
                }
                type="button"
                title={
                    sessao
                        ? 'Meu perfil'
                        : 'Entrar no sistema'
                }
                onClick={abrirPerfil}
            >
                {iniciais}
            </button>
        </header>
    )
}