"""Fluxo de vida das solicitações de serviço: aceite, execução, conclusão e
avaliação, com histórico append-only de eventos.

PONTOS DE INTEGRAÇÃO (módulos futuros dos outros integrantes — nada aqui
depende deles):
- autenticação JWT: `tecnicoId`/`clienteId` hoje chegam no corpo (seletor
  temporário); com JWT passam a vir do token, sem mudar o formato das respostas.
- matching: preencherá `tecnicoId` da solicitação antes do aceite.
- orçamento: pode registrar `valorOrcamento` no aceite — o campo já existe.
- novos status: registrar em `lib/fluxo_servico.py` (`registrar_status`), sem
  tocar neste router.
"""

from datetime import datetime, timezone
from typing import List, Optional

from fastapi import APIRouter, HTTPException
from pymongo import ASCENDING, DESCENDING

from lib.db import db
from lib.fluxo_servico import (
    STATUS_ACEITA,
    STATUS_AVALIADA,
    STATUS_CONCLUIDA,
    STATUS_EXECUCAO,
    STATUS_INICIAL,
    pode_transicionar,
    rotulo_status,
)
from models.solicitacoes import (
    AceiteCreate,
    AvaliacaoCreate,
    ConclusaoCreate,
    EventoSolicitacao,
    InicioCreate,
    Solicitacao,
    SolicitacaoCreate,
)

router = APIRouter(prefix="/solicitacoes", tags=["solicitacoes"])


def _agora() -> datetime:
    return datetime.now(timezone.utc)


async def _carregar_solicitacao(solicitacao_id: str) -> dict:
    documento = await db.solicitacoes.find_one({"id": solicitacao_id})
    if documento is None:
        raise HTTPException(status_code=404, detail="Solicitação não encontrada.")
    return documento


async def _validar_tecnico(tecnico_id: int) -> str:
    documento = await db.tecnicos.find_one({"id": tecnico_id}, {"nomeExibicao": 1})
    if documento is None:
        raise HTTPException(status_code=404, detail="Técnico não encontrado.")
    return documento.get("nomeExibicao", "")


async def _registrar_evento(
    solicitacao_id: str,
    tipo_evento: str,
    descricao: str,
    *,
    status_anterior: Optional[str] = None,
    status_novo: Optional[str] = None,
    autor_id: Optional[int] = None,
    autor_papel: str = "sistema",
) -> None:
    evento = EventoSolicitacao(
        solicitacaoId=solicitacao_id,
        tipoEvento=tipo_evento,
        statusAnterior=status_anterior,
        statusNovo=status_novo,
        descricao=descricao,
        autorId=autor_id,
        autorPapel=autor_papel,
        dataEvento=_agora(),
    )
    await db.eventos_solicitacao.insert_one(evento.model_dump())


@router.get("", response_model=List[Solicitacao])
async def listar(
    clienteId: Optional[int] = None,
    tecnicoId: Optional[int] = None,
    status: Optional[str] = None,
):
    filtros: dict = {}
    if clienteId is not None:
        filtros["clienteId"] = clienteId
    if tecnicoId is not None:
        filtros["tecnicoId"] = tecnicoId
    if status is not None:
        filtros["status"] = status
    documentos = (
        await db.solicitacoes.find(filtros).sort("dataSolicitacao", DESCENDING).to_list(1000)
    )
    return [Solicitacao(**documento) for documento in documentos]


@router.get("/{solicitacao_id}", response_model=Solicitacao)
async def buscar_por_id(solicitacao_id: str):
    documento = await _carregar_solicitacao(solicitacao_id)
    return Solicitacao(**documento)


@router.get("/{solicitacao_id}/historico", response_model=List[EventoSolicitacao])
async def historico(solicitacao_id: str):
    await _carregar_solicitacao(solicitacao_id)
    eventos = (
        await db.eventos_solicitacao.find({"solicitacaoId": solicitacao_id})
        .sort("dataEvento", ASCENDING)
        .to_list(1000)
    )
    return [EventoSolicitacao(**evento) for evento in eventos]


@router.post("", response_model=Solicitacao, status_code=201)
async def criar(dados: SolicitacaoCreate):
    tecnico_nome: Optional[str] = None
    if dados.tecnicoId is not None:
        tecnico_nome = await _validar_tecnico(dados.tecnicoId)

    solicitacao = Solicitacao(
        clienteId=dados.clienteId,
        clienteNome=dados.clienteNome,
        tecnicoId=dados.tecnicoId,
        tecnicoNome=tecnico_nome,
        equipamento=dados.equipamento,
        problemaRelatado=dados.problemaRelatado,
        status=STATUS_INICIAL,
        dataSolicitacao=_agora(),
    )
    await db.solicitacoes.insert_one(solicitacao.model_dump())

    descricao = (
        f"Solicitação aberta por {solicitacao.clienteNome} "
        f'para o equipamento "{solicitacao.equipamento}".'
    )
    await _registrar_evento(
        solicitacao.id,
        "SolicitacaoCriada",
        descricao,
        status_novo=STATUS_INICIAL,
        autor_id=dados.clienteId,
        autor_papel="cliente",
    )
    return solicitacao


@router.patch("/{solicitacao_id}/aceitar", response_model=Solicitacao)
async def aceitar(solicitacao_id: str, dados: AceiteCreate):
    documento = await _carregar_solicitacao(solicitacao_id)
    solicitacao = Solicitacao(**documento)

    if not pode_transicionar(solicitacao.status, STATUS_ACEITA):
        raise HTTPException(
            status_code=400,
            detail=(
                "Não é possível aceitar: a solicitação está no status "
                f'"{rotulo_status(solicitacao.status)}".'
            ),
        )

    if solicitacao.tecnicoId is not None and solicitacao.tecnicoId != dados.tecnicoId:
        raise HTTPException(
            status_code=400,
            detail="O aceite é restrito ao técnico designado para esta solicitação.",
        )

    tecnico_nome = await _validar_tecnico(dados.tecnicoId)

    alteracoes: dict = {
        "status": STATUS_ACEITA,
        "tecnicoId": dados.tecnicoId,
        "tecnicoNome": tecnico_nome,
        "dataAceite": _agora(),
    }
    if dados.valorOrcamento is not None:
        alteracoes["valorOrcamento"] = dados.valorOrcamento

    await db.solicitacoes.update_one({"id": solicitacao_id}, {"$set": alteracoes})

    partes = [f"{tecnico_nome} aceitou a solicitação."]
    if dados.valorOrcamento is not None:
        partes.append(f"Orçamento registrado: R$ {dados.valorOrcamento:.2f}.")
    if dados.mensagem:
        partes.append(dados.mensagem)
    await _registrar_evento(
        solicitacao_id,
        "AceiteRegistrado",
        " ".join(partes),
        status_anterior=solicitacao.status,
        status_novo=STATUS_ACEITA,
        autor_id=dados.tecnicoId,
        autor_papel="tecnico",
    )

    atualizado = await db.solicitacoes.find_one({"id": solicitacao_id})
    return Solicitacao(**atualizado)


@router.patch("/{solicitacao_id}/iniciar", response_model=Solicitacao)
async def iniciar(solicitacao_id: str, dados: InicioCreate):
    documento = await _carregar_solicitacao(solicitacao_id)
    solicitacao = Solicitacao(**documento)

    if not pode_transicionar(solicitacao.status, STATUS_EXECUCAO):
        raise HTTPException(
            status_code=400,
            detail=(
                "Não é possível iniciar o serviço: a solicitação está no status "
                f'"{rotulo_status(solicitacao.status)}".'
            ),
        )

    if solicitacao.tecnicoId != dados.tecnicoId:
        raise HTTPException(
            status_code=400,
            detail="Somente o técnico responsável pode iniciar o serviço.",
        )

    await db.solicitacoes.update_one(
        {"id": solicitacao_id},
        {"$set": {"status": STATUS_EXECUCAO, "dataInicio": _agora()}},
    )

    descricao = f"{solicitacao.tecnicoNome or 'O técnico'} iniciou a execução do serviço."
    if dados.observacao:
        descricao += f" {dados.observacao}"
    await _registrar_evento(
        solicitacao_id,
        "ServicoIniciado",
        descricao,
        status_anterior=solicitacao.status,
        status_novo=STATUS_EXECUCAO,
        autor_id=dados.tecnicoId,
        autor_papel="tecnico",
    )

    atualizado = await db.solicitacoes.find_one({"id": solicitacao_id})
    return Solicitacao(**atualizado)


@router.patch("/{solicitacao_id}/concluir", response_model=Solicitacao)
async def concluir(solicitacao_id: str, dados: ConclusaoCreate):
    documento = await _carregar_solicitacao(solicitacao_id)
    solicitacao = Solicitacao(**documento)

    if not pode_transicionar(solicitacao.status, STATUS_CONCLUIDA):
        raise HTTPException(
            status_code=400,
            detail=(
                "Não é possível concluir o serviço: a solicitação está no status "
                f'"{rotulo_status(solicitacao.status)}".'
            ),
        )

    if solicitacao.tecnicoId != dados.tecnicoId:
        raise HTTPException(
            status_code=400,
            detail="Somente o técnico responsável pode concluir o serviço.",
        )

    alteracoes: dict = {"status": STATUS_CONCLUIDA, "dataConclusao": _agora()}
    if dados.observacaoFinal is not None:
        alteracoes["observacaoFinal"] = dados.observacaoFinal

    await db.solicitacoes.update_one({"id": solicitacao_id}, {"$set": alteracoes})

    descricao = f"{solicitacao.tecnicoNome or 'O técnico'} concluiu o serviço."
    if dados.observacaoFinal:
        descricao += f" {dados.observacaoFinal}"
    await _registrar_evento(
        solicitacao_id,
        "ServicoConcluido",
        descricao,
        status_anterior=solicitacao.status,
        status_novo=STATUS_CONCLUIDA,
        autor_id=dados.tecnicoId,
        autor_papel="tecnico",
    )

    atualizado = await db.solicitacoes.find_one({"id": solicitacao_id})
    return Solicitacao(**atualizado)


@router.post("/{solicitacao_id}/avaliar", response_model=Solicitacao)
async def avaliar(solicitacao_id: str, dados: AvaliacaoCreate):
    documento = await _carregar_solicitacao(solicitacao_id)
    solicitacao = Solicitacao(**documento)

    if solicitacao.clienteId != dados.clienteId:
        raise HTTPException(
            status_code=400,
            detail="Somente o cliente que solicitou o serviço pode avaliá-lo.",
        )

    if not pode_transicionar(solicitacao.status, STATUS_AVALIADA):
        raise HTTPException(
            status_code=400,
            detail=(
                "Só é possível avaliar serviços concluídos: a solicitação está no status "
                f'"{rotulo_status(solicitacao.status)}".'
            ),
        )

    alteracoes: dict = {
        "status": STATUS_AVALIADA,
        "notaAvaliacao": dados.nota,
        "dataAvaliacao": _agora(),
    }
    if dados.comentario is not None:
        alteracoes["comentarioAvaliacao"] = dados.comentario

    await db.solicitacoes.update_one({"id": solicitacao_id}, {"$set": alteracoes})

    descricao = f"Cliente {solicitacao.clienteNome} avaliou o serviço com nota {dados.nota}/5."
    if dados.comentario:
        descricao += f' "{dados.comentario}"'
    await _registrar_evento(
        solicitacao_id,
        "AvaliacaoRegistrada",
        descricao,
        status_anterior=solicitacao.status,
        status_novo=STATUS_AVALIADA,
        autor_id=dados.clienteId,
        autor_papel="cliente",
    )

    atualizado = await db.solicitacoes.find_one({"id": solicitacao_id})
    return Solicitacao(**atualizado)
