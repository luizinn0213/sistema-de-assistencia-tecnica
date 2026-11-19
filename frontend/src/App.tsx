import { useState } from 'react'
import './App.css'
import './execucao-avaliacao.css'
import './matching-ranking.css'

import { Header } from './components/Header'
import type { Pagina } from './components/Header'
import { CadastroCliente } from './pages/CadastroCliente'
import { CadastroTecnico } from './pages/CadastroTecnico'
import { Equipamentos } from './pages/Equipamentos'
import { Especialidades } from './pages/Especialidades'
import { ListaTecnicos } from './pages/ListaTecnicos'
import { Login } from './pages/Login'
import { MatchingTecnicos } from './pages/MatchingTecnicos'
import { MinhasSolicitacoes } from './pages/MinhasSolicitacoes'
import { NovaSolicitacao } from './pages/NovaSolicitacao'
import { PerfilCliente } from './pages/PerfilCliente'
import { PerfilTecnico } from './pages/PerfilTecnico'
import { SolicitacoesTecnico } from './pages/SolicitacoesTecnico'
import { Diagnosticos } from './pages/Diagnosticos'
import { Orcamentos } from './pages/Orcamentos'
import { ExecucaoAvaliacao } from './pages/ExecucaoAvaliacao'
import { TrocarSenha } from './pages/TrocarSenha'
import { obterSessao } from './services/authApi'

function App() {
    const [pagina, setPagina] =
        useState<Pagina>('tecnicos')
    const [sessao, setSessao] = useState(obterSessao)
    const [atendimentoInicial, setAtendimentoInicial] = useState<number>()

    function navegar(destino: Pagina, atendimentoId?: number) {
        setAtendimentoInicial(atendimentoId)
        setSessao(obterSessao())
        setPagina(destino)
    }

    return (
        <>
            <Header
                paginaAtual={pagina}
                aoNavegar={navegar}
                sessao={sessao}
            />

            {pagina === 'tecnicos' && (
                <ListaTecnicos />
            )}

            {pagina === 'matching' && (
                <MatchingTecnicos key={atendimentoInicial} idInicial={atendimentoInicial} />
            )}

            {pagina === 'solicitacoes-tecnico' && (
                <SolicitacoesTecnico aoDiagnosticar={id => navegar('diagnosticos', id)} />
            )}

            {pagina === 'diagnosticos' && (
                <Diagnosticos
                    key={atendimentoInicial}
                    idInicial={atendimentoInicial}
                    aoOrcar={id => navegar('orcamentos', id)}
                />
            )}

            {pagina === 'orcamentos' && (
                <Orcamentos
                    key={atendimentoInicial}
                    idInicial={atendimentoInicial}
                    aoAcompanhar={id => navegar('execucao-avaliacao', id)}
                />
            )}

            {pagina === 'execucao-avaliacao' && (
                <ExecucaoAvaliacao key={atendimentoInicial} idInicial={atendimentoInicial} />
            )}

            {pagina === 'cadastro-tecnico' && (
                <CadastroTecnico aoEntrar={() => setPagina('login')} />
            )}

            {pagina === 'especialidades' && (
                <Especialidades />
            )}

            {pagina === 'perfil-tecnico' && (
                <PerfilTecnico />
            )}

            {pagina === 'cadastro-cliente' && (
                <CadastroCliente />
            )}

            {pagina === 'equipamentos' && (
                <Equipamentos />
            )}

            {pagina === 'login' && (
                <Login aoEntrar={setSessao} />
            )}

            {pagina === 'trocar-senha' && (
                <TrocarSenha aoEntrar={() => navegar('login')} aoTrocar={() => setSessao(null)} />
            )}

            {pagina === 'perfil-cliente' && (
                <PerfilCliente aoTrocarSenha={() => navegar('trocar-senha')} />
            )}

            {pagina === 'nova-solicitacao' && (
                <NovaSolicitacao />
            )}

            {pagina === 'minhas-solicitacoes' && (
                <MinhasSolicitacoes aoEscolherTecnico={solicitacao => navegar('matching', solicitacao.id)} />
            )}
        </>
    )
}

export default App