import axios from 'axios';

export const api = axios.create({ baseURL: '/api' });

const ACCESS_KEY  = 'assistencia.access';
const REFRESH_KEY = 'assistencia.refresh';
const USER_KEY    = 'assistencia.user';

export const tokens = {
  get access()  { return localStorage.getItem(ACCESS_KEY); },
  get refresh() { return localStorage.getItem(REFRESH_KEY); },
  set(access: string, refresh?: string) {
    localStorage.setItem(ACCESS_KEY, access);
    if (refresh) localStorage.setItem(REFRESH_KEY, refresh);
  },
  clear() {
    localStorage.removeItem(ACCESS_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(USER_KEY);
  },
  get user() {
    try { return JSON.parse(localStorage.getItem(USER_KEY) ?? 'null'); }
    catch { return null; }
  },
  setUser(u: any) {
    if (u) localStorage.setItem(USER_KEY, JSON.stringify(u));
    else   localStorage.removeItem(USER_KEY);
  },
};

// Envia Bearer em toda requisição
api.interceptors.request.use((config) => {
  const t = tokens.access;
  if (t) config.headers.Authorization = `Bearer ${t}`;
  return config;
});

// Refresh automático com rotação
let refreshing: Promise<string> | null = null;

api.interceptors.response.use(
  (r) => r,
  async (error) => {
    const original = error.config as any;
    const status   = error.response?.status as number | undefined;
    const url      = String(original?.url ?? '');

    const isAuthRoute = url.includes('/auth/login') || url.includes('/auth/refresh');

    if (status === 401 && !original?._retry && tokens.refresh && !isAuthRoute) {
      original._retry = true;

      refreshing ??= api
        .post('/auth/refresh', { refreshToken: tokens.refresh })
        .then((r) => {
          const d = r.data;
          tokens.set(d.accessToken, d.refreshToken);
          if (d.usuario) tokens.setUser(d.usuario);
          return d.accessToken as string;
        })
        .finally(() => { refreshing = null; });

      try {
        const novo = await refreshing;
        original.headers.Authorization = `Bearer ${novo}`;
        return api(original);
      } catch {
        tokens.clear();
        window.dispatchEvent(new Event('auth:logout'));
      }
    }

    return Promise.reject(error);
  }
);

// Mensagens de erro vindas do backend
export function pegarErro(err: any): string {
  const s = err?.response?.status;
  const d = err?.response?.data;
  if (d?.erro) return String(d.erro);
  if (d?.errors) {
    const arr = Object.values(d.errors).flat() as string[];
    if (arr.length) return arr.join(' ');
  }
  if (d?.title) return String(d.title);
  if (s === 401) return 'Sessão expirada. Faça login novamente.';
  if (s === 403) return 'Você não tem permissão para esta ação.';
  if (s === 429) return 'Muitas tentativas. Aguarde um instante.';
  return 'Erro inesperado.';
}

export const authApi = {
  login: (email: string, senha: string) =>
    api.post('/auth/login', { email, senha }).then((r) => r.data),
  refresh: (refreshToken: string) =>
    api.post('/auth/refresh', { refreshToken }).then((r) => r.data),
  logout: () => api.post('/auth/logout').finally(() => tokens.clear()),
  eu: () => api.get('/auth/eu').then((r) => r.data),
  trocarSenha: (senhaAtual: string, novaSenha: string) =>
    api.put('/auth/senha', { senhaAtual, novaSenha }).then((r) => r.data),
};

export const clientesApi = {
  cadastro: (dto: any) => api.post('/clientes', dto).then((r) => r.data),
  listar: (pagina = 1, tamanho = 10, busca = '') =>
    api.get('/clientes', { params: { pagina, tamanho, busca } }).then((r) => r.data),
  obter: (id: string) => api.get(`/clientes/${id}`).then((r) => r.data),
  atualizar: (id: string, dto: any) =>
    api.put(`/clientes/${id}`, dto).then((r) => r.data),
  remover: (id: string) => api.delete(`/clientes/${id}`).then((r) => r.data),
  meu: () => api.get('/clientes/me').then((r) => r.data),
  atualizarMeu: (dto: any) => api.put('/clientes/me', dto).then((r) => r.data),
};

export const equipamentosApi = {
  meus: () => api.get('/equipamentos/meus').then((r) => r.data),
  criar: (dto: any) => api.post('/equipamentos', dto).then((r) => r.data),
  criarParaCliente: (clienteId: string, dto: any) =>
    api.post(`/equipamentos/cliente/${clienteId}`, dto).then((r) => r.data),
  listar: (pagina = 1, tamanho = 10, busca = '') =>
    api.get('/equipamentos', { params: { pagina, tamanho, busca } }).then((r) => r.data),
  obter: (id: string) => api.get(`/equipamentos/${id}`).then((r) => r.data),
  atualizar: (id: string, dto: any) =>
    api.put(`/equipamentos/${id}`, dto).then((r) => r.data),
  remover: (id: string) => api.delete(`/equipamentos/${id}`).then((r) => r.data),
};

export const usuariosApi = {
  listar: (pagina = 1, tamanho = 10) =>
    api.get('/usuarios', { params: { pagina, tamanho } }).then((r) => r.data),
  criar: (dto: any) => api.post('/usuarios', dto).then((r) => r.data),
  alterarPerfil: (id: string, perfil: string) =>
    api.put(`/usuarios/${id}/perfil`, { perfil }).then((r) => r.data),
  alterarStatus: (id: string, ativo: boolean) =>
    api.put(`/usuarios/${id}/status`, { ativo }).then((r) => r.data),
  remover: (id: string) => api.delete(`/usuarios/${id}`).then((r) => r.data),
};