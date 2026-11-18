import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth';

export default function Layout() {
  const { user, logout, isAdmin, isCliente } = useAuth();
  const nav = useNavigate();

  async function sair() {
    await logout();
    nav('/login', { replace: true });
  }

  return (
    <div className="app">
      <aside className="sidebar">
        <h1>Assistência Técnica</h1>
        <p className="user">
          {user?.nome} <small>({user?.perfil})</small>
        </p>
        <nav>
          <NavLink to="/" end>Início</NavLink>
          {isCliente && <NavLink to="/meu-perfil">Meu perfil</NavLink>}
          {isCliente && <NavLink to="/meus-equipamentos">Meus equipamentos</NavLink>}
          {!isCliente && <NavLink to="/clientes">Clientes</NavLink>}
          {!isCliente && <NavLink to="/equipamentos">Equipamentos</NavLink>}
          {isAdmin && <NavLink to="/usuarios">Usuários</NavLink>}
          <NavLink to="/senha">Trocar senha</NavLink>
        </nav>
        <button className="btn-sair" onClick={sair}>Sair</button>
      </aside>
      <main className="content"><Outlet /></main>
    </div>
  );
}