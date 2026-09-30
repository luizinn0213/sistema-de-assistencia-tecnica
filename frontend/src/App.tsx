import { useState } from 'react'
import './App.css'
import { Header } from './components/Header'
import type { Pagina } from './components/Header'
import { CadastroCliente } from './pages/CadastroCliente'
import { CadastroTecnico } from './pages/CadastroTecnico'
import { Equipamentos } from './pages/Equipamentos'
import { Especialidades } from './pages/Especialidades'
import { ListaTecnicos } from './pages/ListaTecnicos'
import { Login } from './pages/Login'
import { MatchingTecnicos } from './pages/MatchingTecnicos'
import { PerfilCliente } from './pages/PerfilCliente'
import { PerfilTecnico } from './pages/PerfilTecnico'
import { SolicitacoesTecnico } from './pages/SolicitacoesTecnico'

function App() {
    const [pagina, setPagina] =
        useState<Pagina>('tecnicos')

    return (
        <>
            <Header
                paginaAtual={pagina}
                aoNavegar={setPagina}
            />

            {pagina === 'tecnicos' && (
                <ListaTecnicos />
            )}

            {pagina === 'matching' && (
                <MatchingTecnicos />
            )}

            {pagina === 'solicitacoes-tecnico' && (
                <SolicitacoesTecnico />
            )}

            {pagina === 'cadastro-tecnico' && (
                <CadastroTecnico />
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
                <Login />
            )}

            {pagina === 'perfil-cliente' && (
                <PerfilCliente />
            )}
        </>
    )
}

export default App