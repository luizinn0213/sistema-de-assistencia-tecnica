"""Dados iniciais do módulo de serviços (idempotente).

`cd /app/backend && python seed.py` — executa apenas se as coleções estiverem
vazias. Os técnicos espelham o contrato do módulo .NET da equipe (mesmos
campos, ids inteiros); as solicitações cobrem todos os status do fluxo
(aceite, execução, conclusão e avaliação) com seus eventos de histórico.
"""

import asyncio
from datetime import datetime, timedelta, timezone

from lib.db import db, ensure_indexes


def _ha(horas: float) -> datetime:
    """Momento X horas atrás, em UTC aware (a ordenação do histórico depende disso)."""
    return datetime.now(timezone.utc) - timedelta(hours=horas)


TECNICOS = [
    {
        "id": 1,
        "usuarioId": 101,
        "nomeExibicao": "Carlos Mendes",
        "descricaoProfissional": "Especialista em refrigeração e climatização residencial.",
        "cidadeAtendimento": "São Paulo",
        "estadoAtendimento": "SP",
        "ativo": True,
        "disponivel": True,
        "dataCadastro": _ha(720),
        "especialidades": [
            {"especialidadeId": 1, "nome": "Refrigeração", "nivelExperiencia": "Especialista", "anosExperiencia": 12},
            {"especialidadeId": 2, "nome": "Climatização", "nivelExperiencia": "Avancado", "anosExperiencia": 9},
        ],
    },
    {
        "id": 2,
        "usuarioId": 102,
        "nomeExibicao": "Fernanda Lopes",
        "descricaoProfissional": "Consertos de eletrodomésticos em geral, atendimento em domicílio.",
        "cidadeAtendimento": "Rio de Janeiro",
        "estadoAtendimento": "RJ",
        "ativo": True,
        "disponivel": False,
        "dataCadastro": _ha(600),
        "especialidades": [
            {"especialidadeId": 3, "nome": "Eletrodomésticos", "nivelExperiencia": "Avancado", "anosExperiencia": 8},
        ],
    },
    {
        "id": 3,
        "usuarioId": 103,
        "nomeExibicao": "Ricardo Alves",
        "descricaoProfissional": "Eletricista predial e residencial, emergências 24 horas.",
        "cidadeAtendimento": "Belo Horizonte",
        "estadoAtendimento": "MG",
        "ativo": True,
        "disponivel": True,
        "dataCadastro": _ha(540),
        "especialidades": [
            {"especialidadeId": 4, "nome": "Elétrica", "nivelExperiencia": "Avancado", "anosExperiencia": 10},
        ],
    },
    {
        "id": 4,
        "usuarioId": 104,
        "nomeExibicao": "Juliana Prado",
        "descricaoProfissional": "Assistência técnica de informática e redes.",
        "cidadeAtendimento": "Curitiba",
        "estadoAtendimento": "PR",
        "ativo": True,
        "disponivel": True,
        "dataCadastro": _ha(480),
        "especialidades": [
            {"especialidadeId": 5, "nome": "Informática", "nivelExperiencia": "Especialista", "anosExperiencia": 7},
        ],
    },
]


def _evento(
    solicitacao_id: str,
    tipo: str,
    descricao: str,
    *,
    momento: datetime,
    status_anterior: str | None = None,
    status_novo: str | None = None,
    autor_id: int | None = None,
    autor_papel: str = "sistema",
) -> dict:
    return {
        "id": f"ev-{solicitacao_id}-{tipo}",
        "solicitacaoId": solicitacao_id,
        "tipoEvento": tipo,
        "statusAnterior": status_anterior,
        "statusNovo": status_novo,
        "descricao": descricao,
        "autorId": autor_id,
        "autorPapel": autor_papel,
        "dataEvento": momento,
    }


SOLICITACOES = [
    {
        "id": "sol-seed-01",
        "clienteId": 501,
        "clienteNome": "Ana Souza",
        "tecnicoId": None,
        "tecnicoNome": None,
        "equipamento": "Geladeira Brastemp frost free",
        "problemaRelatado": "Geladeira não está gelando e está formando gelo no congelador.",
        "valorOrcamento": None,
        "status": "AguardandoAceite",
        "dataSolicitacao": _ha(20),
        "dataAceite": None,
        "dataInicio": None,
        "dataConclusao": None,
        "observacaoFinal": None,
        "notaAvaliacao": None,
        "comentarioAvaliacao": None,
        "dataAvaliacao": None,
    },
    {
        "id": "sol-seed-02",
        "clienteId": 502,
        "clienteNome": "Bruno Lima",
        "tecnicoId": 2,
        "tecnicoNome": "Fernanda Lopes",
        "equipamento": "Máquina de lavar Electrolux",
        "problemaRelatado": "Máquina não completa o ciclo de centrifugação.",
        "valorOrcamento": 350.0,
        "status": "Aceita",
        "dataSolicitacao": _ha(72),
        "dataAceite": _ha(60),
        "dataInicio": None,
        "dataConclusao": None,
        "observacaoFinal": None,
        "notaAvaliacao": None,
        "comentarioAvaliacao": None,
        "dataAvaliacao": None,
    },
    {
        "id": "sol-seed-03",
        "clienteId": 501,
        "clienteNome": "Ana Souza",
        "tecnicoId": 3,
        "tecnicoNome": "Ricardo Alves",
        "equipamento": "Disjuntor do chuveiro",
        "problemaRelatado": "O disjuntor desarma sempre que o chuveiro é ligado.",
        "valorOrcamento": 180.0,
        "status": "EmExecucao",
        "dataSolicitacao": _ha(96),
        "dataAceite": _ha(90),
        "dataInicio": _ha(30),
        "dataConclusao": None,
        "observacaoFinal": None,
        "notaAvaliacao": None,
        "comentarioAvaliacao": None,
        "dataAvaliacao": None,
    },
    {
        "id": "sol-seed-04",
        "clienteId": 502,
        "clienteNome": "Bruno Lima",
        "tecnicoId": 1,
        "tecnicoNome": "Carlos Mendes",
        "equipamento": "Ar-condicionado split 12000 BTU",
        "problemaRelatado": "Ar não esfria e apresenta vazamento de água na unidade interna.",
        "valorOrcamento": 420.0,
        "status": "Concluida",
        "dataSolicitacao": _ha(120),
        "dataAceite": _ha(110),
        "dataInicio": _ha(72),
        "dataConclusao": _ha(12),
        "observacaoFinal": "Troca do capacitor e limpeza completa do evaporador.",
        "notaAvaliacao": None,
        "comentarioAvaliacao": None,
        "dataAvaliacao": None,
    },
    {
        "id": "sol-seed-05",
        "clienteId": 501,
        "clienteNome": "Ana Souza",
        "tecnicoId": 2,
        "tecnicoNome": "Fernanda Lopes",
        "equipamento": "Fogão Consul",
        "problemaRelatado": "Forno não aquece na temperatura correta.",
        "valorOrcamento": 260.0,
        "status": "Avaliada",
        "dataSolicitacao": _ha(240),
        "dataAceite": _ha(200),
        "dataInicio": _ha(190),
        "dataConclusao": _ha(150),
        "observacaoFinal": "Substituição da resistência do forno.",
        "notaAvaliacao": 5,
        "comentarioAvaliacao": "Atendimento rápido e serviço bem feito.",
        "dataAvaliacao": _ha(140),
    },
]

EVENTOS = [
    _evento(
        "sol-seed-01",
        "SolicitacaoCriada",
        'Solicitação aberta por Ana Souza para o equipamento "Geladeira Brastemp frost free".',
        momento=_ha(20),
        status_novo="AguardandoAceite",
        autor_id=501,
        autor_papel="cliente",
    ),
    _evento(
        "sol-seed-02",
        "SolicitacaoCriada",
        'Solicitação aberta por Bruno Lima para o equipamento "Máquina de lavar Electrolux".',
        momento=_ha(72),
        status_novo="AguardandoAceite",
        autor_id=502,
        autor_papel="cliente",
    ),
    _evento(
        "sol-seed-02",
        "AceiteRegistrado",
        "Fernanda Lopes aceitou a solicitação. Orçamento registrado: R$ 350.00. Consigo atender amanhã pela manhã.",
        momento=_ha(60),
        status_anterior="AguardandoAceite",
        status_novo="Aceita",
        autor_id=2,
        autor_papel="tecnico",
    ),
    _evento(
        "sol-seed-03",
        "SolicitacaoCriada",
        'Solicitação aberta por Ana Souza para o equipamento "Disjuntor do chuveiro".',
        momento=_ha(96),
        status_novo="AguardandoAceite",
        autor_id=501,
        autor_papel="cliente",
    ),
    _evento(
        "sol-seed-03",
        "AceiteRegistrado",
        "Ricardo Alves aceitou a solicitação. Orçamento registrado: R$ 180.00.",
        momento=_ha(90),
        status_anterior="AguardandoAceite",
        status_novo="Aceita",
        autor_id=3,
        autor_papel="tecnico",
    ),
    _evento(
        "sol-seed-03",
        "ServicoIniciado",
        "Ricardo Alves iniciou a execução do serviço. Peças já em mãos.",
        momento=_ha(30),
        status_anterior="Aceita",
        status_novo="EmExecucao",
        autor_id=3,
        autor_papel="tecnico",
    ),
    _evento(
        "sol-seed-04",
        "SolicitacaoCriada",
        'Solicitação aberta por Bruno Lima para o equipamento "Ar-condicionado split 12000 BTU".',
        momento=_ha(120),
        status_novo="AguardandoAceite",
        autor_id=502,
        autor_papel="cliente",
    ),
    _evento(
        "sol-seed-04",
        "AceiteRegistrado",
        "Carlos Mendes aceitou a solicitação. Orçamento registrado: R$ 420.00.",
        momento=_ha(110),
        status_anterior="AguardandoAceite",
        status_novo="Aceita",
        autor_id=1,
        autor_papel="tecnico",
    ),
    _evento(
        "sol-seed-04",
        "ServicoIniciado",
        "Carlos Mendes iniciou a execução do serviço.",
        momento=_ha(72),
        status_anterior="Aceita",
        status_novo="EmExecucao",
        autor_id=1,
        autor_papel="tecnico",
    ),
    _evento(
        "sol-seed-04",
        "ServicoConcluido",
        "Carlos Mendes concluiu o serviço. Troca do capacitor e limpeza completa do evaporador.",
        momento=_ha(12),
        status_anterior="EmExecucao",
        status_novo="Concluida",
        autor_id=1,
        autor_papel="tecnico",
    ),
    _evento(
        "sol-seed-05",
        "SolicitacaoCriada",
        'Solicitação aberta por Ana Souza para o equipamento "Fogão Consul".',
        momento=_ha(240),
        status_novo="AguardandoAceite",
        autor_id=501,
        autor_papel="cliente",
    ),
    _evento(
        "sol-seed-05",
        "AceiteRegistrado",
        "Fernanda Lopes aceitou a solicitação. Orçamento registrado: R$ 260.00.",
        momento=_ha(200),
        status_anterior="AguardandoAceite",
        status_novo="Aceita",
        autor_id=2,
        autor_papel="tecnico",
    ),
    _evento(
        "sol-seed-05",
        "ServicoIniciado",
        "Fernanda Lopes iniciou a execução do serviço.",
        momento=_ha(190),
        status_anterior="Aceita",
        status_novo="EmExecucao",
        autor_id=2,
        autor_papel="tecnico",
    ),
    _evento(
        "sol-seed-05",
        "ServicoConcluido",
        "Fernanda Lopes concluiu o serviço. Substituição da resistência do forno.",
        momento=_ha(150),
        status_anterior="EmExecucao",
        status_novo="Concluida",
        autor_id=2,
        autor_papel="tecnico",
    ),
    _evento(
        "sol-seed-05",
        "AvaliacaoRegistrada",
        'Cliente Ana Souza avaliou o serviço com nota 5/5. "Atendimento rápido e serviço bem feito."',
        momento=_ha(140),
        status_anterior="Concluida",
        status_novo="Avaliada",
        autor_id=501,
        autor_papel="cliente",
    ),
]


async def main() -> None:
    tecnicos_existentes = await db.tecnicos.count_documents({})
    solicitacoes_existentes = await db.solicitacoes.count_documents({})

    if tecnicos_existentes or solicitacoes_existentes:
        print("Seed ignorado: já existem dados nas coleções.")
        return

    await db.tecnicos.insert_many(TECNICOS)
    await db.solicitacoes.insert_many(SOLICITACOES)
    await db.eventos_solicitacao.insert_many(EVENTOS)
    await ensure_indexes()

    print(
        f"Seed concluído: {len(TECNICOS)} técnicos, "
        f"{len(SOLICITACOES)} solicitações, {len(EVENTOS)} eventos."
    )


if __name__ == "__main__":
    asyncio.run(main())
