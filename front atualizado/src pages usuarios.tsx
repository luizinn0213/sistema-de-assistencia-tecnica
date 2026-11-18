import { useEffect, useState, FormEvent } from 'react';
import { usuariosApi, pegarErro } from '../api';

const PERFIS = ['Cliente', 'Tecnico', 'Administrador'] as const;

export default function Usuarios() {
  const [lista, setLista] = useState<any[]>([]);
  const [total, setTotal] = useState(0);
  const [pagina, setPagina] = useState(1);
  const [erro, setErro] = useState('');
  const [criando, setCriando] = useState(false);
  const [form, setForm] = useState({ nome: '', email: '', senha: '', perfil: 'Tecnico' });

  async function carregar(p = pagina) {
    setErro('');
    try {
      const d = await usuariosApi.listar(p, 10);
      setLista(d.itens ?? d.items ?? d.dados ?? []);
      setTotal(d.total ?? 0);
      setPagina(p);
    } catch (e) { setErro(pegarErro(e)); }
  }
  useEffect(() => { carregar(1); /* eslint-disable-next-line */ }, []);

  async function cadastrar(ev: FormEvent) {
    ev.preventDefault(); setErro('');
    try {
      await usuariosApi.criar(form);
      setCriando(false);
      setForm({ nome: '', email: '', senha: '', perfil: 'Tecnico' });
      carregar(1);
    } catch (e) { setErro(pegarErro(e)); }
  }

  async function mudarPerfil(id: string, perfil: string) {
    try { await usuariosApi.alterarPerfil(id, perfil); carregar(); }
    catch (e) { setErro(pegarErro(e)); }
  }
  async function alternarStatus(u: any) {
    try { await usuariosApi.alterarStatus(u.id, !u.ativo); carregar(); }
    catch (e) { setErro(pegarErro(e)); }
  }
  async function remover(id: string) {
    if (!confirm('Excluir usuário?')) return;
    try { await usuariosApi.remover(id); carregar(); }
    catch (e) { setErro(pegarErro(e)); }
  }

  const totalPaginas = Math.max(1, Math.ceil(total / 10));

  return (
    <div className="painel">
      <h2>Usuários</h2>
      {erro && <div className="erro">{erro}</div>}
      <div className="barra">
        <button className="btn" onClick={() => setCriando(true)}>Novo usuário</button>
      </div>
      <table className="tabela">
        <thead>
          <tr><th>Nome</th><th>E-mail</th><th>Perfil</th><th>Ativo</th><th></th></tr>
        </thead>
        <tbody>
          {lista.map((u) => (
            <tr key={u.id}>
              <td>{u.nome}</td><td>{u.email}</td>
              <td>
                <select value={u.perfil} onChange={(e) => mudarPerfil(u.id, e.target.value)}>
                  {PERFIS.map((p) => <option key={p} value={p}>{p}</option>)}
                </select>
              </td>
              <td>{u.ativo ? 'Sim' : 'Não'}</td>
              <td className="acoes">
                <button onClick={() => alternarStatus(u)}>
                  {u.ativo ? 'Desativar' : 'Ativar'}
                </button>
                <button className="perigo" onClick={() => remover(u.id)}>Excluir</button>
              </td>
            </tr>
          ))}
          {lista.length === 0 && <tr><td colSpan={5}>Nenhum usuário.</td></tr>}
        </tbody>
      </table>
      <div className="paginacao">
        <button disabled={pagina <= 1} onClick={() => carregar(pagina - 1)}>Anterior</button>
        <span>Página {pagina} de {totalPaginas}</span>
        <button disabled={pagina >= totalPaginas} onClick={() => carregar(pagina + 1)}>Próxima</button>
      </div>

      {criando && (
        <form className="card" onSubmit={cadastrar}>
          <h3>Novo usuário</h3>
          <div className="grade">
            <label>Nome
              <input value={form.nome}
                onChange={(e) => setForm({ ...form, nome: e.target.value })} required />
            </label>
            <label>E-mail
              <input type="email" value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })} required />
            </label>
            <label>Senha
              <input type="password" value={form.senha}
                onChange={(e) => setForm({ ...form, senha: e.target.value })}
                required minLength={6} />
            </label>
            <label>Perfil
              <select value={form.perfil}
                onChange={(e) => setForm({ ...form, perfil: e.target.value })}>
                {PERFIS.map((p) => <option key={p} value={p}>{p}</option>)}
              </select>
            </label>
          </div>
          <div className="barra">
            <button className="btn">Cadastrar</button>
            <button type="button" className="btn-sec" onClick={() => setCriando(false)}>Cancelar</button>
          </div>
        </form>
      )}
    </div>
  );
}