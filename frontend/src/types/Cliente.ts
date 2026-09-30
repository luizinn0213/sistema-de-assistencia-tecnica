export interface Endereco {
    id?: number
    clienteId?: number
    cep: string
    logradouro: string
    numero: string
    bairro: string
    cidade: string
    estado: string
}

export interface EquipamentoResumo {
    id: number
    clienteId: number
    tipo: string
    marca: string
    modelo: string
    numeroSerie: string
}

export interface Cliente {
    id: number
    usuarioId: number
    nome: string
    email: string
    telefone?: string
    cpfCnpj: string
    endereco?: Endereco
    equipamentos: EquipamentoResumo[]
}

export interface CriarClienteDados {
    nome: string
    email: string
    senha: string
    telefone?: string
    cpfCnpj: string
    endereco: Endereco
}

export interface ClienteCadastrado {
    id: number
    usuarioId: number
    nome: string
    email: string
    mensagem: string
}

export interface AtualizarClienteDados {
    nome: string
    telefone?: string
    endereco: Endereco
}