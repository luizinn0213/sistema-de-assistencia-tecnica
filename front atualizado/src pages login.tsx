import { FormEvent, useState } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth';
import { pegarErro } from '../api';

export default function Login() {
  const { login, user } = useAuth();
  const nav = useNavigate();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(false);

  if (user) return <Navigate to="/" replace />;

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setErro(''); setCarregando(true);
    try {
      await login(email, senha);
      nav('/', { replace: true });
    } catch (err) {
      setErro(pegarErro(err));
    } finally {
      setCarregando(false);
    }
  }

  return (
    <div className="centro">
      <form className="card" onSubmit={onSubmit} style={{ maxWidth: 420 }}>
        <h2>Entrar</h2>
        {erro && <div className="erro">{erro}</div>}
        <label>E-mail
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </label>
        <label>Senha
          <input type="password" value={senha} onChange={(e) => setSenha(e.target.value)} required />
        </label>
        <button className="btn" disabled={carregando}>
          {carregando ? 'Entrando…' : 'Entrar'}
        </button>
        <p className="rodape">Não tem conta? <Link to="/cadastro">Cadastre-se</Link></p>
      </form>
    </div>
  );
}