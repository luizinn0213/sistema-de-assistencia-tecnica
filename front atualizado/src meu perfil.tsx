import { useEffect, useState, FormEvent } from 'react';
import { clientesApi, pegarErro } from '../api';

export default function MeuPerfil() {
  const [dados, setDados] = useState<any>(null);
  const [erro, setErro] = useState('');
  const [ok, setOk] = useState('');
  const [carregando, setCarregando] = useState(true);
  const [salvando, setSalvando] = useState(false);

  useEffect(() => {
    clientesApi.meu()
      .then(setDados)
      .catch((e) => setErro(pegarErro(e)))
      .finally(() => setCarregando(false));
  }, []);

  const set = (k: string) =>
    (e: React.ChangeEvent<HTMLInputElement>) => setDados({ ...dados, [k]: e.target.value });
  const setEnd = (k: string) =>
    (e: React.ChangeEvent<HTMLInputElement>) =>
      setDados({ ...dados, endereco: { ...(dados.endereco ?? {}), [k]: e.target.value } });

  async function salvar(ev: FormEvent) {
    ev.preventDefault();
    setErro(''); setOk(''); setSalvando(true);
    try {
      const atualizado = await clientesApi.atualizarMeu({
        nome: dados.nome, telefone: dados.telefone, endereco: dados.endereco,
      });
      setDados(atualizado);
      setOk('Dados atualizados.');
    } catch (e) { setErro(pegarErro(e)); }
    finally { setSalvando(false); }
  }

  if (carregando) return <p>Carregando…</p>;
  if (erro && !dados) return <div className="erro">{erro}</div>;

  return (
    <form className="card painel" onSubmit={salvar}>
      <h2>Meu perfil</h2>
      {erro && <div className="erro">{erro}</div>}
      {ok && <div className="ok">{ok}</div>}
      <div className="grade">
        <label>Nome<input value={dados.nome ?? ''} onChange={set('nome')} required /></label>
        <label>E-mail<input value={dados.email ?? ''} disabled /></label>
        <label>CPF/CNPJ<input value={dados.cpfCnpj ?? ''} onChange={set('cpfCnpj')} /></label>
        <label>Telefone<input value={dados.telefone ?? ''} onChange={set('telefone')} /></label>
      </div>
      <h3>Endereço</h3>
      <div className="grade">
        <label>CEP<input value={dados.endereco?.cep ?? ''} onChange={setEnd('cep')} /></label>
        <label>Logradouro<input value={dados.endereco?.logradouro ?? ''} onChange={setEnd('logradouro')} /></label>
        <label>Número<input value={dados.endereco?.numero ?? ''} onChange={setEnd('numero')} /></label>
        <label>Complemento<input value={dados.endereco?.complemento ?? ''} onChange={setEnd('complemento')} /></label>
        <label>Bairro<input value={dados.endereco?.bairro ?? ''} onChange={setEnd('bairro')} /></label>
        <label>Cidade<input value={dados.endereco?.cidade ?? ''} onChange={setEnd('cidade')} /></label>
        <label>UF<input value={dados.endereco?.estado ?? ''} onChange={setEnd('estado')} maxLength={2} /></label>
      </div>
      <button className="btn" disabled={salvando}>{salvando ? 'Salvando…' : 'Salvar'}</button>
    </form>
  );
}