import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authApi, pegarErro } from '../api';
import { useAuth } from '../auth';

export default function TrocarSenha() {
  const { logout } = useAuth();
  const nav = useNavigate();
  const [senhaAtual, setSenhaAtual] = useState('');
  const [novaSenha, setNovaSenha] = useState('');
  const [confirmar, setConfirmar] = useState('');
  const [erro, setErro] = useState('');
  const [ok, setOk] = useState('');
  const [enviando, setEnviando] = useState(false);

  async function onSubmit(ev: FormEvent) {
    ev.preventDefault();
    setErro(''); setOk('');
    if (novaSenha !== confirmar) { setErro('As senhas não coincidem.'); return; }
    setEnviando(true);
    try {
      await authApi.trocarSenha(senhaAtual, novaSenha);
      setOk('Senha alterada. Você será deslogado.');
      await logout();
      setTimeout(() => nav('/login', { replace: true }), 800);
    } catch (e) { setErro(pegarErro(e)); }
    finally { setEnviando(false); }
  }

  return (
    <form className="card painel" onSubmit={onSubmit} style={{ maxWidth: 480 }}>
      <h2>Trocar senha</h2>
      {erro && <div className="erro">{erro}</div>}
      {ok && <div className="ok">{ok}</div>}
      <label>Senha atual
        <input type="password" value={senhaAtual}
          onChange={(e) => setSenhaAtual(e.target.value)} required />
      </label>
      <label>Nova senha
        <input type="password" value={novaSenha}
          onChange={(e) => setNovaSenha(e.target.value)} required minLength={6} />
      </label>
      <label>Confirmar nova senha
        <input type="password" value={confirmar}
          onChange={(e) => setConfirmar(e.target.value)} required />
      </label>
      <button className="btn" disabled={enviando}>
        {enviando ? 'Enviando…' : 'Alterar senha'}
      </button>
    </form>
  );
}