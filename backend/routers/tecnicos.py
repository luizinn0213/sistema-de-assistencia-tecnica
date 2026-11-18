"""Leitura de técnicos (contrato espelhado do módulo .NET da equipe).

PONTO DE INTEGRAÇÃO: o módulo completo de Técnicos (CRUD, disponibilidade,
especialidades) já existe no repositório da equipe. Este router expõe apenas
LEITURA sobre a coleção `tecnicos`, com a mesma forma de resposta do controller
`api/tecnicos`, para o módulo de serviços funcionar de pé. Ao integrar o módulo
oficial, remova este arquivo e a linha que o inclui em server.py — os contratos
de resposta são idênticos.
"""

from datetime import datetime
from typing import List

from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from pymongo import ASCENDING

from lib.db import db
from models.solicitacoes import ModeloDatas

router = APIRouter(prefix="/tecnicos", tags=["tecnicos"])


class EspecialidadeResumo(BaseModel):
    especialidadeId: int
    nome: str
    nivelExperiencia: str
    anosExperiencia: int


class TecnicoResumo(ModeloDatas):
    id: int
    usuarioId: int
    nomeExibicao: str
    descricaoProfissional: str
    cidadeAtendimento: str
    estadoAtendimento: str
    ativo: bool
    disponivel: bool
    dataCadastro: datetime
    especialidades: List[EspecialidadeResumo] = []


@router.get("", response_model=List[TecnicoResumo])
async def listar():
    documentos = await db.tecnicos.find().sort("nomeExibicao", ASCENDING).to_list(100)
    return [TecnicoResumo(**documento) for documento in documentos]


@router.get("/{tecnico_id}", response_model=TecnicoResumo)
async def buscar_por_id(tecnico_id: int):
    documento = await db.tecnicos.find_one({"id": tecnico_id})
    if documento is None:
        raise HTTPException(status_code=404, detail="Técnico não encontrado.")
    return TecnicoResumo(**documento)
