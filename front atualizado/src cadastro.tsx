[import { FormEvent, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { clientesApi, pegarErro } from '../api';

export default function Cadastro() {
  const nav = useNavigate();
  const [form, setForm] = useState({
    nome: '', email: '', senha: '', cpfCnpj: '', telefone: '',
    cep: '', logradouro: '', numero: '', complemento: '', bairro: '', cidade: '', estado: '',
  });
  const [erro, setErro] = useState('');
  const [ok, setOk] = useState('');
  const [carregando, setCarregando] = useState(false);

  const set = (k: keyof typeof form) =>
    (e: React.ChangeEvent<HTMLInputElement>) => setForm({ ...form, [k]: e.target.value });

  async function onSubmit(ev: FormEvent) {
    ev.preventDefault();
    setErro(''); setOk(''); setCarregando(true);
    try {
      const temEndereco = form.cep || form.logradouro || form.cidade;
      const endereco = temEndereco ? {
        cep: form.cep, logradouro: form.logradouro, numero: form.numero,
        complemento: form.complemento, bairro: form.bairro,
        cidade: form.cidade, estado: form.estado,
      } : undefined;

      await clientesApi.cadastro({
        nome: form.nome, email: form.email, senha: form.senha,
        cpfCnpj: form.cpfCnpj, telefone: form.telefone, endereco,
      });
      setOk('Cadastro realizado! Redirecionando para o login…');
      setTimeout(() => nav('/login'), 1200);
    } catch (err) {
      setErro(pegarErro(err));
    } finally { setCarregando(false); }
  }

  return (
    <div className="centro">
      <form className="card" onSubmit={onSubmit}>
        <h2>Cadastro de cliente</h2>
        {erro && <div className="erro">{erro}</div>}
        {ok   && <div className="ok">{ok}</div>}
        <div className="grade">
          <label>Nome<input value={form.nome} onChange={set('nome')} required /></label>
          <label>E-mail<input type="email" value={form.email} onChange={set('email')} required /></label>
          <label>Senha<input type="password" value={form.senha} onChange={set('senha')} required minLength={6} /></label>
          <label>CPF/CNPJ<input value={form.cpfCnpj} onChange={set('cpfCnpj')} required /></label>
          <label>Telefone<input value={form.telefone} onChange={set('telefone')} required /></label>
        </div>
        <h3>Endereço (opcional)</h3>
        <div className="grade">
          <label>CEP<input value={form.cep} onChange={set('cep')} /></label>
          <label>Logradouro<input value={form.logradouro} onChange={set('logradouro')} /></label>
          <label>Número<input value={form.numero} onChange={set('numero')} /></label>
          <label>Complemento<input value={form.complemento} onChange={set('complemento')} /></label>
          <label>Bairro<input value={form.bairro} onChange={set('bairro')} /></label>
          <label>Cidade<input value={form.cidade} onChange={set('cidade')} /></label>
          <label>UF<input value={form.estado} onChange={set('estado')} maxLength={2} /></label>
        </div>
        <button className="btn" disabled={carregando}>
          {carregando ? 'Enviando…' : 'Cadastrar'}
        </button>
        <p className="rodape"><Link to="/login">Voltar ao login</Link></p>
      </form>
    </div>
  );
}