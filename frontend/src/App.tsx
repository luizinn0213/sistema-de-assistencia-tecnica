import { useState } from 'react'
import './App.css'
import { Header } from './components/Header'
import type { Pagina } from './components/Header'
import { ListaTecnicos } from './pages/ListaTecnicos'
import { CadastroTecnico } from './pages/CadastroTecnico'
import { Especialidades } from './pages/Especialidades'
import { PerfilTecnico } from './pages/PerfilTecnico'
import { MatchingTecnicos } from './pages/MatchingTecnicos'
import { CadastroCliente } from './pages/CadastroCliente'
import { Equipamentos } from './pages/Equipamentos'
import { Login } from './pages/Login'
import { PerfilCliente } from './pages/PerfilCliente'

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