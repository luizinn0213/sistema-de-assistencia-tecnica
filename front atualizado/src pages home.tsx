import { useAuth } from '../auth';

export default function Home() {
  const { user, isAdmin, isTecnico, isCliente } = useAuth();
  return (
    <div className="painel">
      <h2>Olá, {user?.nome}</h2>
      <p>Você está logado como <strong>{user?.perfil}</strong>.</p>
      <ul>
        {isCliente && <li>Use <em>Meu perfil</em> e <em>Meus equipamentos</em> no menu lateral.</li>}
        {isTecnico && <li>Gerencie <em>Clientes</em> e <em>Equipamentos</em>.</li>}
        {isAdmin && <li>Você também pode gerenciar <em>Usuários</em> e excluir cadastros.</li>}
      </ul>
    </div>
  );
}