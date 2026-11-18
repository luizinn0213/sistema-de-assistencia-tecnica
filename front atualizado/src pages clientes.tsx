import { useEffect, useState, FormEvent } from 'react';
import { clientesApi, pegarErro } from '../api';
import { useAuth } from '../auth';

export default function Clientes() {
  const { isAdmin } = useAuth();
  const [lista, setLista] = useState<any[]>([]);
  const [total, setTotal] = useState(0);
  const [pagina, setPagina] = useState(1);
  const [busca, setBusca] = useState('');
  const [erro, setErro] = useState('');
  const [editando, setEditando] = useState<any>(null);

  async function carregar(p = pagina) {
    setErro('');
    try {
      const d = await clientesApi.listar(p, 10, busca);
      setLista(d.itens ?? d.items ?? d.dados ?? []);
      setTotal(d.total ?? 0);
      setPagina(p);
    } catch (e) { setErro(pegarErro(e)); }
  }

  useEffect(() => { carregar(1); /* eslint-disable-next-line */ }, []);

  async function salvar(ev: FormEvent) {
    ev.preventDefault();
    try {
      await clientesApi.atualizar(editando.id, {
        nome: editando.nome, telefone: editando.telefone, endereco: editando.endereco,
      });
      setEditando(null);
      carregar();
    } catch (e) { setErro(pegarErro(e)); }
  }

  async function remover(id: string) {
    if (!confirm('Excluir cliente?')) return;
    try { await clientesApi.remover(id); carregar(); }
    catch (e) { setErro(pegarErro(e)); }
  }

  const totalPaginas = Math.max(1, Math.ceil(total / 10));

  return (
    <div className="painel">
      <h2>Clientes</h2>
      {erro && <div className="erro">{erro}</div>}
      <div className="barra">
        <input placeholder="Buscar…" value={busca}
          onChange={(e) => setBusca(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && carregar(1)} />
        <button className="btn" onClick={() => carregar(1)}>Buscar</button>
      </div>
      <table className="tabela">
        <thead><tr><th>Nome</th><th>E-mail</th><th>Telefone</th><th></th></tr></thead>
        <tbody>
          {lista.map((c) => (
            <tr key={c.id}>
              <td>{c.nome}</td><td>{c.email}</td><td>{c.telefone}</td>
              <td className="acoes">
                <button onClick={() => setEditando({ ...c })}>Editar</button>
                {isAdmin && <button className="perigo" onClick={() => remover(c.id)}>Excluir</button>}
              </td>
            </tr>
          ))}
          {lista.length === 0 && <tr><td colSpan={4}>Nenhum cliente.</td></tr>}
        </tbody>
      </table>
      <div className="paginacao">
        <button disabled={pagina <= 1} onClick={() => carregar(pagina - 1)}>Anterior</button>
        <span>Página {pagina} de {totalPaginas} — {total} cliente(s)</span>
        <button disabled={pagina >= totalPaginas} onClick={() => carregar(pagina + 1)}>Próxima</button>
      </div>

      {editando && (
        <form className="card" onSubmit={salvar}>
          <h3>Editar cliente</h3>
          <label>Nome
            <input value={editando.nome ?? ''}
              onChange={(e) => setEditando({ ...editando, nome: e.target.value })} />
          </label>
          <label>Telefone
            <input value={editando.telefone ?? ''}
              onChange={(e) => setEditando({ ...editando, telefone: e.target.value })} />
          </label>
          <div className="barra">
            <button className="btn">Salvar</button>
            <button type="button" className="btn-sec" onClick={() => setEditando(null)}>Cancelar</button>
          </div>
        </form>
      )}
    </div>
  );
}