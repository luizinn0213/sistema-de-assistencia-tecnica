export interface Atendimento {
    id: number
    numero: string
    status: string
    equipamento: {
        tipo: string
        marca: string
        modelo: string
    }
    especialidade: {
        id: number
        nome: string
    }
    cidade: string | null
    diagnosticoId: number | null
    diagnosticoStatus: string | null
    orcamentoId: number | null
    orcamentoStatus: string | null
}
