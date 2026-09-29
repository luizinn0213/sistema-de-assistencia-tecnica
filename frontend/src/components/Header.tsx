export type Pagina =
  | 'tecnicos'
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
        onClick={() => aoNavegar('tecnicos')}
      >
        <span>NG</span>
        OS NARGGETS
      </button>

      <nav className="menu-principal">
        <button
          className={paginaAtual === 'tecnicos' ? 'ativo' : ''}
          onClick={() => aoNavegar('tecnicos')}
        >
          Buscar técnicos
        </button>

        <button
          className={
            paginaAtual === 'cadastro-tecnico' ? 'ativo' : ''
          }
          onClick={() => aoNavegar('cadastro-tecnico')}
        >
          Cadastrar técnico
        </button>

        <button
        className={
            paginaAtual === 'perfil-tecnico' ? 'ativo' : ''
        }
        onClick={() => aoNavegar('perfil-tecnico')}
        >
        Meu perfil técnico
        </button>

        <button
          className={
            paginaAtual === 'especialidades' ? 'ativo' : ''
          }
          onClick={() => aoNavegar('especialidades')}
        >
          Especialidades
        </button>

        <button
          disabled
          title="Será integrado ao módulo de clientes"
        >
          Meus equipamentos
        </button>
      </nav>

      <button className="botao-perfil" title="Perfil do usuário">
        LC
      </button>
    </header>
  )
}