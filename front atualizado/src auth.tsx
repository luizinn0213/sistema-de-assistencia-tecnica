import { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { authApi, tokens } from './api';

export type Perfil = 'Cliente' | 'Tecnico' | 'Administrador';

export interface Usuario {
  id: string;
  nome: string;
  email: string;
  perfil: Perfil;
  clienteId?: string | null;
  ativo?: boolean;
}

interface AuthCtx {
  user: Usuario | null;
  loading: boolean;
  login: (email: string, senha: string) => Promise<void>;
  logout: () => Promise<void>;
  isAdmin: boolean;
  isTecnico: boolean;
  isCliente: boolean;
}

const Ctx = createContext<AuthCtx>(null as any);
export const useAuth = () => useContext(Ctx);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<Usuario | null>(tokens.user);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function init() {
      if (!tokens.access) { setLoading(false); return; }
      try {
        const u = await authApi.eu();
        setUser(u);
        tokens.setUser(u);
      } catch {
        tokens.clear();
        setUser(null);
      } finally {
        setLoading(false);
      }
    }
    init();

    const onLogout = () => setUser(null);
    window.addEventListener('auth:logout', onLogout);
    return () => window.removeEventListener('auth:logout', onLogout);
  }, []);

  async function login(email: string, senha: string) {
    const d = await authApi.login(email, senha);
    tokens.set(d.accessToken, d.refreshToken);
    tokens.setUser(d.usuario);
    setUser(d.usuario);
  }

  async function logout() {
    try { await authApi.logout(); } catch { tokens.clear(); }
    setUser(null);
  }

  const perfil = user?.perfil;
  return (
    <Ctx.Provider value={{
      user, loading, login, logout,
      isAdmin:   perfil === 'Administrador',
      isTecnico: perfil === 'Tecnico',
      isCliente: perfil === 'Cliente',
    }}>
      {children}
    </Ctx.Provider>
  );
}