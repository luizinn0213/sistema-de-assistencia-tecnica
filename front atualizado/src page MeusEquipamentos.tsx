import { useEffect, useState, FormEvent } from 'react';
import { equipamentosApi, pegarErro } from '../api';

export default function MeusEquipamentos() {
  const [lista, setLista] = useState<any[]>([]);
  const [erro, setErro] = useState('');
  const [form, setForm] = useState({ tipo: '', marca: '', modelo: '', numeroSerie: '', descricao: '' });
  const [enviando, setEnviando] = useState(false);

  async function carregar() {
    try { setLista(await equipamentosApi.meus()); }
    catch (e) { setErro(pegarErro(e)); }
  }
  useEffect(() => { carregar(); }, []);

  const set = (k: keyof typeof form) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) =>
      setForm({ ...form, [k]: e.target.value });

  async function cadastrar(ev: FormEvent) {
    ev.preventDefault(); setErro(''); setEnviando(true);
    try {
      await equipamentosApi.criar(form);
      setForm({ tipo: '', marca: '', modelo: '', numeroSerie: '', descricao: '' });
      carregar();
    } catch (e) { setErro(pegarErro(e)); }
    finally { setEnviando(false); }
  }

  async function remover(id: string) {
    if (!confirm('Excluir equipamento?')) return;
    try { await equipamentosApi.remover(id); carregar(); }
    catch (e) { setErro(pegarErro(e)); }
  }

  return (
    <div className="painel">
      <h2>Meus equipamentos</h2>
      {erro && <div className="erro">{erro}</div>}

      <form className="card" onSubmit={cadastrar}>
        <h3>Cadastrar novo</h3>
        <div className="grade">
          <label>Tipo<input value={form.tipo} onChange={set('tipo')} required /></label>
          <label>Marca<input value={form.marca} onChange={set('marca')} /></label>
          <label>Modelo<input value={form.modelo} onChange={set('modelo')} /></label>
          <label>Nº de série<input value={form.numeroSerie} onChange={set('numeroSerie')} /></label>
        </div>
        <label>Descrição
          <textarea value={form.descricao} onChange={set('descricao')} rows={3} />
        </label>
        <button className="btn" disabled={enviando}>{enviando ? 'Salvando…' : 'Cadastrar'}</button>
      </form>

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
    </div>
  );
}