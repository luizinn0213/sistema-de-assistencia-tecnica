import { ReactNode } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './auth';
import Layout from './pages/Layout';
import Login from './pages/Login';
import Cadastro from './pages/Cadastro';
import Home from './pages/Home';
import MeuPerfil from './pages/MeuPerfil';
import Clientes from './pages/Clientes';
import MeusEquipamentos from './pages/MeusEquipamentos';
import Equipamentos from './pages/Equipamentos';
import Usuarios from './pages/Usuarios';
import TrocarSenha from './pages/TrocarSenha';

function RotaProtegida({
  children,
  perfis,
}: { children: ReactNode; perfis?: string[] }) {
  const { user, loading } = useAuth();
  if (loading) return <div className="centro">Carregando…</div>;
  if (!user) return <Navigate to="/login" replace />;
  if (perfis && !perfis.includes(user.perfil)) return <Navigate to="/" replace />;
  return <>{children}</>;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/cadastro" element={<Cadastro />} />

      <Route
        path="/"
        element={
          <RotaProtegida>
            <Layout />
          </RotaProtegida>
        }
      >
        <Route index element={<Home />} />

        <Route path="meu-perfil"
          element={<RotaProtegida perfis={['Cliente']}><MeuPerfil /></RotaProtegida>} />
        <Route path="meus-equipamentos"
          element={<RotaProtegida perfis={['Cliente']}><MeusEquipamentos /></RotaProtegida>} />

        <Route path="clientes"
          element={<RotaProtegida perfis={['Tecnico', 'Administrador']}><Clientes /></RotaProtegida>} />
        <Route path="equipamentos"
          element={<RotaProtegida perfis={['Tecnico', 'Administrador']}><Equipamentos /></RotaProtegida>} />

        <Route path="usuarios"
          element={<RotaProtegida perfis={['Administrador']}><Usuarios /></RotaProtegida>} />

        <Route path="senha" element={<TrocarSenha />} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}