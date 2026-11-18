import { Link, NavLink } from "react-router-dom";

// Módulos dos outros integrantes: ficam desabilitados até a integração final
// (mesmo padrão do "Meus equipamentos" do layout original).
const MODULOS_EXTERNOS = [
  { rotulo: "Cadastrar técnico", aviso: "Módulo de outro integrante — será integrado futuramente" },
  { rotulo: "Meu perfil técnico", aviso: "Módulo de outro integrante — será integrado futuramente" },
  { rotulo: "Especialidades", aviso: "Módulo de outro integrante — será integrado futuramente" },
  { rotulo: "Meus equipamentos", aviso: "Será integrado ao módulo de clientes" },
];

export default function Header() {
  return (
    <header className="barra-superior">
      <Link to="/" className="logo" data-testid="logo-os-narggets">
        <span>NG</span>
        OS NARGGETS
      </Link>

      <nav className="menu-principal">
        <NavLink
          to="/solicitacoes"
          className={({ isActive }) => (isActive ? "ativo" : "")}
          data-testid="nav-servicos"
        >
          Serviços
        </NavLink>

        <NavLink
          to="/"
          end
          className={({ isActive }) => (isActive ? "ativo" : "")}
          data-testid="nav-buscar-tecnicos"
        >
          Buscar técnicos
        </NavLink>

        {MODULOS_EXTERNOS.map((modulo) => (
          <button
            key={modulo.rotulo}
            disabled
            title={modulo.aviso}
            data-testid={`nav-modulo-externo-${modulo.rotulo
              .toLowerCase()
              .replace(/\s+/g, "-")}`}
          >
            {modulo.rotulo}
          </button>
        ))}
      </nav>

      <button className="botao-perfil" title="Perfil do usuário" data-testid="botao-perfil">
        LC
      </button>
    </header>
  );
}
