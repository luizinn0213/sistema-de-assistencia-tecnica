import { useEffect, useState, FormEvent } from 'react';
import { clientesApi, equipamentosApi, pegarErro } from '../api';

export default function Equipamentos() {
  const [lista, setLista] = useState<any[]>([]);
  const [total, setTotal] = useState(0);
  const [pagina, setPagina] = useState(1);
  const [busca, setBusca] = useState('');
  const [erro, setErro] = useState('');
  const [criando, setCriando] = useState(false);
  const [clientes, setClientes] = useState<any[]>([]);
  const [form, setForm] = useState({
    clienteId: '', tipo: '', marca: '', modelo: '', numeroSerie: '', descricao: '',
  });

  async function carregar(p = pagina) {
    setErro('');
    try {
      const d = await equipamentosApi.listar(p, 10, busca);
      setLista(d.itens ?? d.items ?? d.dados ?? []);
      setTotal(d.total ?? 0);
      setPagina(p);
    } catch (e) { setErro(pegarErro(e)); }
  }

  useEffect(() => { carregar(1); /* eslint-disable-next-line */ }, []);

  async function abrirNovo() {
    setCriando(true);
    try {
      const d = await clientesApi.listar(1, 200, '');
      setClientes(d.itens ?? d.items ?? d.dados ?? []);
    } catch (e) { setErro(pegarErro(e)); }
  }

  const set = (k: keyof typeof form) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) =>
      setForm({ ...form, [k]: e.target.value });

  async function cadastrar(ev: FormEvent) {
    ev.preventDefault(); setErro('');
    try {
      const { clienteId, ...dto } = form;
      await equipamentosApi.criarParaCliente(clienteId, dto);
      setCriando(false);
      setForm({ clienteId: '', tipo: '', marca: '', modelo: '', numeroSerie: '', descricao: '' });
      carregar(1);
    } catch (e) { setErro(pegarErro(e)); }
  }

  async function remover(id: string) {
    if (!confirm('Excluir equipamento?')) return;
    try { await equipamentosApi.remover(id); carregar(); }
    catch (e) { setErro(pegarErro(e)); }
  }

  const totalPaginas = Math.max(1, Math.ceil(total / 10));

  return (
    <div className="painel">
      <h2>Equipamentos</h2>
      {erro && <div className="erro">{erro}</div>}
      <div className="barra">
        <input placeholder="Buscar…" value={busca}
          onChange={(e) => setBusca(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && carregar(1)} />
        <button className="btn" onClick={() => carregar(1)}>Buscar</button>
        <button className="btn-sec" onClick={abrirNovo}>Novo para cliente</button>
      </div>
      <table className="tabela">
        <thead><tr><th>Tipo</th><th>Marca</th><th>Modelo</th><th>Série</th><th></th></tr></thead>
        <tbody>
          {lista.map((e) => (
            <tr key={e.id}>
              <td>{e.tipo}</td><td>{e.marca}</td><td>{e.modelo}</td><td>{e.numeroSerie}</td>
              <td className="acoes">
                <button className="perigo" onClick={() => remover(e.id)}>Excluir</button>
              </td>
            </tr>
          ))}
          {lista.length === 0 && <tr><td colSpan={5}>Nenhum equipamento.</td></tr>}
        </tbody>
      </table>
      <div className="paginacao">
        <button disabled={pagina <= 1} onClick={() => carregar(pagina - 1)}>Anterior</button>
        <span>Página {pagina} de {totalPaginas} — {total}</span>
        <button disabled={pagina >= totalPaginas} onClick={() => carregar(pagina + 1)}>Próxima</button>
      </div>

      {criando && (
        <form className="card" onSubmit={cadastrar}>
          <h3>Novo equipamento para cliente</h3>
          <label>Cliente
            <select value={form.clienteId} onChange={set('clienteId')} required>
              <option value="">Selecione…</option>
              {clientes.map((c) => (
                <option key={c.id} value={c.id}>{c.nome} ({c.email})</option>
              ))}
            </select>
          </label>
          <div className="grade">
            <label>Tipo<input value={form.tipo} onChange={set('tipo')} required /></label>
            <label>Marca<input value={form.marca} onChange={set('marca')} /></label>
            <label>Modelo<input value={form.modelo} onChange={set('modelo')} /></label>
            <label>Nº série<input value={form.numeroSerie} onChange={set('numeroSerie')} /></label>
          </div>
          <label>Descrição
            <textarea value={form.descricao} onChange={set('descricao')} rows={3} />
          </label>
          <div className="barra">
            <button className="btn">Salvar</button>
            <button type="button" className="btn-sec" onClick={() => setCriando(false)}>Cancelar</button>
          </div>
        </form>
      )}
    </div>
  );
}