export type Pagina =
    | 'tecnicos'
    | 'matching'
    | 'cadastro-tecnico'
    | 'perfil-tecnico'
    | 'especialidades'

interface HeaderProps {
    paginaAtual: Pagina
    aoNavegar: (pagina: Pagina) => void
}

export function Header({
    paginaAtual,
    aoNavegar
}: HeaderProps) {
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
                    onClick={() =>
                        aoNavegar('tecnicos')
                    }
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
                    onClick={() =>
                        aoNavegar('matching')
                    }
                >
                    Escolher técnico
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
                    Meu perfil técnico
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
                    type="button"
                    disabled
                    title="Será integrado ao módulo de clientes"
                >
                    Meus equipamentos
                </button>
            </nav>

            <button
                className="botao-perfil"
                type="button"
                title="Perfil do usuário"
            >
                LC
            </button>
        </header>
    )
}