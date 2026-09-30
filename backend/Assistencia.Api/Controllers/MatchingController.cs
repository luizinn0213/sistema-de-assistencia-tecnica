using Assistencia.Api.Data;
using Assistencia.Api.Dtos;
using Assistencia.Api.Enums;
using Assistencia.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/matching")]
public class MatchingController : ControllerBase
{
    private readonly AppDbContext _context;

    public MatchingController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("tecnicos")]
    public async Task<ActionResult> BuscarTecnicos(
        int especialidadeId,
        string? cidade = null)
    {
        var tecnicos = await _context.Tecnicos
            .Include(t => t.TecnicoEspecialidades)
            .ThenInclude(te => te.Especialidade)
            .AsNoTracking()
            .Where(t =>
                t.Ativo &&
                t.Disponivel &&
                t.TecnicoEspecialidades.Any(te =>
                    te.EspecialidadeId == especialidadeId &&
                    te.Especialidade != null &&
                    te.Especialidade.Ativa))
            .ToListAsync();

        var resultado = tecnicos
            .Select(tecnico =>
            {
                var vinculo = tecnico.TecnicoEspecialidades
                    .First(te =>
                        te.EspecialidadeId == especialidadeId);

                var mesmaCidade =
                    !string.IsNullOrWhiteSpace(cidade) &&
                    string.Equals(
                        tecnico.CidadeAtendimento,
                        cidade,
                        StringComparison.OrdinalIgnoreCase);

                var pontuacao =
                    50 +
                    ((int)vinculo.NivelExperiencia * 10) +
                    Math.Min(vinculo.AnosExperiencia, 10) +
                    (mesmaCidade ? 30 : 0);

                return new
                {
                    tecnico.Id,
                    tecnico.NomeExibicao,
                    tecnico.DescricaoProfissional,
                    tecnico.CidadeAtendimento,
                    tecnico.EstadoAtendimento,
                    tecnico.Disponivel,

                    EspecialidadeId = vinculo.EspecialidadeId,
                    Especialidade = vinculo.Especialidade!.Nome,

                    NivelExperiencia =
                        vinculo.NivelExperiencia.ToString(),

                    vinculo.AnosExperiencia,
                    MesmaCidade = mesmaCidade,
                    Pontuacao = pontuacao
                };
            })
            .OrderByDescending(t => t.Pontuacao)
            .ThenBy(t => t.NomeExibicao)
            .ToList();

        return Ok(resultado);
    }

    [HttpPost("selecionar")]
    public async Task<ActionResult> SelecionarTecnico(
        SelecionarTecnicoDto dados)
    {
        var selecaoExistente = await _context.SelecoesTecnicos
            .AnyAsync(s =>
                s.SolicitacaoId == dados.SolicitacaoId &&
                (s.Status ==
                    StatusSelecaoTecnico.AguardandoResposta ||
                 s.Status ==
                    StatusSelecaoTecnico.Aceita));

        if (selecaoExistente)
        {
            return Conflict(
                "Essa solicitação já possui um técnico selecionado.");
        }

        var tecnico = await _context.Tecnicos
            .Include(t => t.TecnicoEspecialidades)
            .FirstOrDefaultAsync(t =>
                t.Id == dados.TecnicoId);

        if (tecnico is null)
        {
            return NotFound("Técnico não encontrado.");
        }

        var especialidade = await _context.Especialidades
            .FirstOrDefaultAsync(e =>
                e.Id == dados.EspecialidadeId);

        if (especialidade is null)
        {
            return NotFound("Especialidade não encontrada.");
        }

        var possuiEspecialidade =
            tecnico.TecnicoEspecialidades.Any(te =>
                te.EspecialidadeId ==
                    dados.EspecialidadeId);

        if (!possuiEspecialidade)
        {
            return BadRequest(
                "O técnico não possui essa especialidade.");
        }

        try
        {
            var selecao = new SelecaoTecnico(
                dados.SolicitacaoId,
                tecnico,
                especialidade);

            _context.SelecoesTecnicos.Add(selecao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(BuscarSelecaoDaSolicitacao),
                new
                {
                    solicitacaoId = selecao.SolicitacaoId
                },
                new
                {
                    selecao.Id,
                    selecao.SolicitacaoId,
                    selecao.TecnicoId,
                    Tecnico = tecnico.NomeExibicao,
                    selecao.EspecialidadeId,
                    Especialidade = especialidade.Nome,
                    Status = selecao.Status.ToString(),
                    selecao.DataSelecao
                });
        }
        catch (InvalidOperationException erro)
        {
            return BadRequest(erro.Message);
        }
        catch (ArgumentException erro)
        {
            return BadRequest(erro.Message);
        }
    }

    [HttpGet("solicitacoes/{solicitacaoId:int}")]
    public async Task<ActionResult>
        BuscarSelecaoDaSolicitacao(int solicitacaoId)
    {
        var selecao = await _context.SelecoesTecnicos
            .Include(s => s.Tecnico)
            .Include(s => s.Especialidade)
            .AsNoTracking()
            .Where(s =>
                s.SolicitacaoId == solicitacaoId)
            .OrderByDescending(s => s.DataSelecao)
            .FirstOrDefaultAsync();

        if (selecao is null)
        {
            return NotFound(
                "Nenhuma seleção encontrada para essa solicitação.");
        }

        return Ok(new
        {
            selecao.Id,
            selecao.SolicitacaoId,
            selecao.TecnicoId,
            Tecnico = selecao.Tecnico!.NomeExibicao,
            selecao.EspecialidadeId,
            Especialidade = selecao.Especialidade!.Nome,
            Status = selecao.Status.ToString(),
            selecao.DataSelecao,
            selecao.DataResposta
        });
    }
}