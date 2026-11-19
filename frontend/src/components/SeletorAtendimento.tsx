import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { listarAtendimentos } from '../services/atendimentosApi'
import { obterSessao } from '../services/authApi'
import type { Atendimento } from '../types/Atendimento'

type Etapa = 'matching' | 'diagnostico' | 'orcamento' | 'execucao'
type Perfil = 'Cliente' | 'Tecnico'

const estadosVazios: Record<Etapa, string> = {
    matching: 'Abra uma solicitação para escolher um técnico.',
    diagnostico: 'Aceite uma solicitação recebida para iniciar o diagnóstico.',
    orcamento: 'Os atendimentos com orçamento disponível aparecerão aqui.',
    execucao: 'Os atendimentos com orçamento aprovado aparecerão aqui.'
}

function disponivel(atendimento: Atendimento, etapa: Etapa, perfil: Perfil) {
    if (atendimento.status === 'Cancelada') return false

    switch (etapa) {
        case 'matching':
            return ['Aberta', 'AguardandoTecnico'].includes(atendimento.status)
        case 'diagnostico':
            return true
        case 'orcamento':
            return perfil === 'Tecnico'
                ? atendimento.diagnosticoStatus === 'Concluido' || Boolean(atendimento.orcamentoId)
                : Boolean(atendimento.orcamentoId)
        case 'execucao':
            return atendimento.orcamentoStatus === 'Aprovado'
    }
}

interface Props {
    etapa: Etapa
    idInicial?: number
    children: (atendimento: Atendimento, perfil: Perfil) => ReactNode
}

export function SeletorAtendimento({ etapa, idInicial, children }: Props) {
    const sessao = obterSessao()
    const perfil = sessao?.tipo
    const perfilPermitido = perfil === 'Cliente' || perfil === 'Tecnico'
    const permitido = perfilPermitido &&
        (etapa !== 'matching' || perfil === 'Cliente') &&
        (etapa !== 'diagnostico' || perfil === 'Tecnico')
    const usuarioId = permitido ? sessao?.id : undefined
    const [atendimentos, setAtendimentos] = useState<Atendimento[] | null>(null)
    const [escolhido, setEscolhido] = useState<number | undefined>(idInicial)
    const [erro, setErro] = useState('')
    const [versao, setVersao] = useState(0)

    function atualizar() {
        setAtendimentos(null)
        setErro('')
        setVersao(valor => valor + 1)
    }

    useEffect(() => {
        if (!usuarioId) return

        const controle = new AbortController()
        listarAtendimentos(controle.signal).then(setAtendimentos).catch((falha: unknown) => {
            if (!controle.signal.aborted) {
                setErro(falha instanceof Error ? falha.message : 'Não foi possível carregar seus atendimentos.')
            }
        })

        return () => controle.abort()
    }, [usuarioId, versao])

    if (!permitido) {
        const conta = etapa === 'matching' ? ' de cliente' : etapa === 'diagnostico' ? ' de técnico' : ''
        return <p className="estado-vazio">Entre com sua conta{conta} para continuar.</p>
    }
    if (!atendimentos && !erro) return <p role="status">Carregando atendimentos...</p>

    const opcoes = atendimentos?.filter(item => disponivel(item, etapa, perfil)) ?? []
    const atendimento = opcoes.find(item => item.id === escolhido) ??
        (opcoes.length === 1 ? opcoes[0] : undefined)

    return (
        <>
            <div className="acoes-equipamento seletor-acoes">
                <button type="button" className="botao-secundario" onClick={atualizar}>
                    Atualizar atendimentos
                </button>
            </div>
            {erro && <p className="mensagem erro" role="alert">{erro}</p>}
            {!erro && opcoes.length === 0 && <p className="estado-vazio">{estadosVazios[etapa]}</p>}
            {opcoes.length > 0 && (
                <label className="seletor-atendimento">
                    Solicitação
                    <select
                        value={atendimento?.id ?? ''}
                        onChange={evento => setEscolhido(Number(evento.target.value))}
                    >
                        <option value="" disabled>Selecione uma solicitação</option>
                        {opcoes.map(item => (
                            <option key={item.id} value={item.id}>
                                {item.numero} · {item.equipamento.tipo} {item.equipamento.marca} {item.equipamento.modelo}
                            </option>
                        ))}
                    </select>
                </label>
            )}
            {atendimento && children(atendimento, perfil)}
        </>
    )
}
