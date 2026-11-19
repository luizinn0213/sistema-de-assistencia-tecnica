using Assistencia.Api.Data;
using Assistencia.Api.Enums;
using Assistencia.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/atendimentos")]
public class AtendimentosController(AppDbContext context, IUsuarioAtual usuario) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Listar(CancellationToken cancellationToken)
    {
        if (usuario.UsuarioId is not int usuarioId)
        {
            return Unauthorized();
        }

        var solicitacoes = context.Solicitacoes.AsNoTracking();
        var cliente = usuario.Perfil == nameof(TipoUsuario.Cliente);

        if (cliente)
        {
            solicitacoes = solicitacoes.Where(s => s.Cliente!.UsuarioId == usuarioId);
        }
        else if (usuario.Perfil == nameof(TipoUsuario.Tecnico))
        {
            solicitacoes = solicitacoes.Where(s => context.SelecoesTecnicos.Any(selecao =>
                selecao.SolicitacaoId == s.Id &&
                selecao.Status == StatusSelecaoTecnico.Aceita &&
                selecao.Tecnico!.UsuarioId == usuarioId));
        }
        else
        {
            return Forbid();
        }

        var atendimentos = await (
            from solicitacao in solicitacoes
            join diagnostico in context.Diagnosticos on solicitacao.Id equals diagnostico.SolicitacaoId into diagnosticos
            from diagnostico in diagnosticos.DefaultIfEmpty()
            join orcamento in context.Orcamentos on diagnostico.Id equals orcamento.DiagnosticoId into orcamentos
            from orcamento in orcamentos.DefaultIfEmpty()
            let orcamentoVisivel = orcamento != null && (!cliente || orcamento.DataEnvio != null)
            orderby solicitacao.DataCriacao descending, solicitacao.Id descending
            select new
            {
                solicitacao.Id,
                solicitacao.Numero,
                Status = solicitacao.Status.ToString(),
                Equipamento = new
                {
                    solicitacao.Equipamento!.Tipo,
                    solicitacao.Equipamento!.Marca,
                    solicitacao.Equipamento!.Modelo
                },
                Especialidade = new
                {
                    solicitacao.Especialidade!.Id,
                    solicitacao.Especialidade!.Nome
                },
                Cidade = cliente ? solicitacao.Cliente!.Endereco!.Cidade : null,
                DiagnosticoId = !cliente || orcamentoVisivel ? (int?)diagnostico!.Id : null,
                DiagnosticoStatus = !cliente ? (StatusDiagnostico?)diagnostico!.Status : null,
                OrcamentoId = orcamentoVisivel ? (int?)orcamento!.Id : null,
                OrcamentoStatus = orcamentoVisivel ? (StatusOrcamento?)orcamento!.Status : null
            }).ToListAsync(cancellationToken);

        return Ok(atendimentos.Select(a => new
        {
            a.Id,
            a.Numero,
            a.Status,
            a.Equipamento,
            a.Especialidade,
            a.Cidade,
            a.DiagnosticoId,
            DiagnosticoStatus = a.DiagnosticoStatus?.ToString(),
            a.OrcamentoId,
            OrcamentoStatus = a.OrcamentoStatus?.ToString()
        }));
    }
}
