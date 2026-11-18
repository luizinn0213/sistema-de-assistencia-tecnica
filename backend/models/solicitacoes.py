"""Contratos do módulo de serviços (solicitações).

Os nomes de campo são intencionalmente em camelCase: eles cruzam a fronteira
HTTP com as mesmas chaves JSON dos modelos .NET do restante da equipe,
mantendo os contratos compatíveis para a integração final.
"""

from datetime import datetime, timezone
from typing import Optional
import uuid

from pydantic import BaseModel, Field, field_validator


class ModeloDatas(BaseModel):
    """Normaliza datetimes ingênuos vindos do Mongo para UTC (serializa com offset)."""

    @field_validator("*", mode="before")
    @classmethod
    def datas_aware(cls, valor: object) -> object:
        if isinstance(valor, datetime) and valor.tzinfo is None:
            return valor.replace(tzinfo=timezone.utc)
        return valor


class TextosLimpos(BaseModel):
    """Recorta espaços e recusa texto vazio — mesma regra dos modelos .NET."""

    @field_validator("*", mode="before")
    @classmethod
    def limpar_textos(cls, valor: object) -> object:
        if isinstance(valor, str):
            valor = valor.strip()
            if not valor:
                raise ValueError("campo de texto obrigatório")
        return valor


class Solicitacao(ModeloDatas):
    id: str = Field(default_factory=lambda: str(uuid.uuid4()))
    clienteId: int
    clienteNome: str
    tecnicoId: Optional[int] = None
    tecnicoNome: Optional[str] = None
    equipamento: str
    problemaRelatado: str
    valorOrcamento: Optional[float] = None
    status: str
    dataSolicitacao: datetime
    dataAceite: Optional[datetime] = None
    dataInicio: Optional[datetime] = None
    dataConclusao: Optional[datetime] = None
    observacaoFinal: Optional[str] = None
    notaAvaliacao: Optional[int] = None
    comentarioAvaliacao: Optional[str] = None
    dataAvaliacao: Optional[datetime] = None


class EventoSolicitacao(ModeloDatas):
    id: str = Field(default_factory=lambda: str(uuid.uuid4()))
    solicitacaoId: str
    tipoEvento: str
    statusAnterior: Optional[str] = None
    statusNovo: Optional[str] = None
    descricao: str
    autorId: Optional[int] = None
    autorPapel: str
    dataEvento: datetime


class SolicitacaoCreate(TextosLimpos):
    clienteId: int = Field(gt=0)
    clienteNome: str = Field(min_length=1, max_length=120)
    tecnicoId: Optional[int] = Field(default=None, gt=0)
    equipamento: str = Field(min_length=1, max_length=160)
    problemaRelatado: str = Field(min_length=1, max_length=2000)


class AceiteCreate(TextosLimpos):
    tecnicoId: int = Field(gt=0)
    valorOrcamento: Optional[float] = Field(default=None, gt=0)
    mensagem: Optional[str] = Field(default=None, max_length=500)


class InicioCreate(TextosLimpos):
    tecnicoId: int = Field(gt=0)
    observacao: Optional[str] = Field(default=None, max_length=500)


class ConclusaoCreate(TextosLimpos):
    tecnicoId: int = Field(gt=0)
    observacaoFinal: Optional[str] = Field(default=None, max_length=1000)


class AvaliacaoCreate(TextosLimpos):
    clienteId: int = Field(gt=0)
    nota: int = Field(ge=1, le=5)
    comentario: Optional[str] = Field(default=None, max_length=1000)
