"""Fluxo de status das solicitações de serviço.

PONTO DE EXTENSÃO: os novos status trazidos pelos outros integrantes entram
aqui, em um único lugar, sem tocar nos routers. Basta chamar
`registrar_status(...)` (ou adicionar entradas nos dicionários) e o restante do
fluxo passa a conhecê-lo.
"""

STATUS_INICIAL = "AguardandoAceite"
STATUS_ACEITA = "Aceita"
STATUS_EXECUCAO = "EmExecucao"
STATUS_CONCLUIDA = "Concluida"
STATUS_AVALIADA = "Avaliada"

# Rótulos exibidos na interface (chave interna -> texto em PT-BR).
LABELS_STATUS: dict[str, str] = {
    STATUS_INICIAL: "Aguardando aceite",
    STATUS_ACEITA: "Aceita",
    STATUS_EXECUCAO: "Em execução",
    STATUS_CONCLUIDA: "Concluída",
    STATUS_AVALIADA: "Avaliada",
}

# Grafo de transições: status atual -> destinos permitidos.
TRANSICOES: dict[str, set[str]] = {
    STATUS_INICIAL: {STATUS_ACEITA},
    STATUS_ACEITA: {STATUS_EXECUCAO},
    STATUS_EXECUCAO: {STATUS_CONCLUIDA},
    STATUS_CONCLUIDA: {STATUS_AVALIADA},
}

# Tipos de evento do histórico (chave interna -> rótulo em PT-BR).
LABELS_EVENTO: dict[str, str] = {
    "SolicitacaoCriada": "Solicitação criada",
    "AceiteRegistrado": "Aceite registrado",
    "ServicoIniciado": "Serviço iniciado",
    "ServicoConcluido": "Serviço concluído",
    "AvaliacaoRegistrada": "Avaliação registrada",
}


def rotulo_status(status: str) -> str:
    """Rótulo de exibição; status desconhecido (futuro) aparece como veio."""
    return LABELS_STATUS.get(status, status)


def pode_transicionar(status_atual: str, novo_status: str) -> bool:
    """A ação só é válida se o grafo de transições permitir."""
    return novo_status in TRANSICOES.get(status_atual, set())


def registrar_status(
    status: str,
    *,
    rotulo: str | None = None,
    destinos: set[str] | None = None,
    origens: set[str] | None = None,
) -> None:
    """Integração futura: registra um status novo (ex.: "Cancelada").

    - `rotulo`: texto exibido na interface.
    - `destinos`: para quais status ele pode avançar.
    - `origens`: de quais status é possível chegar a ele.
    """
    LABELS_STATUS.setdefault(status, rotulo or status)
    TRANSICOES.setdefault(status, set())
    for destino in destinos or ():
        TRANSICOES[status].add(destino)
    for origem in origens or ():
        TRANSICOES.setdefault(origem, set()).add(status)
