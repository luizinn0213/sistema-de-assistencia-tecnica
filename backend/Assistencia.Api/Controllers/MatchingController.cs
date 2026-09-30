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

        var solicitacao = await _context.Solicitacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == dados.SolicitacaoId);

        if (solicitacao is null)
        {
            return NotFound("Solicitação não encontrada.");
        }

        if (solicitacao.EspecialidadeId !=
            dados.EspecialidadeId)
        {
            return BadRequest(
                "A especialidade informada não corresponde à solicitação.");
        }

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

    [HttpGet("tecnicos/{tecnicoId:int}/pendentes")]
    public async Task<ActionResult> ListarSelecoesPendentes(
        int tecnicoId)
    {
        var selecoes = await _context.SelecoesTecnicos
            .Include(s => s.Especialidade)
            .AsNoTracking()
            .Where(s =>
                s.TecnicoId == tecnicoId &&
                s.Status ==
                    StatusSelecaoTecnico.AguardandoResposta)
            .OrderBy(s => s.DataSelecao)
            .Select(s => new
            {
                s.Id,
                s.SolicitacaoId,
                s.TecnicoId,
                s.EspecialidadeId,
                Especialidade = s.Especialidade!.Nome,
                Status = s.Status.ToString(),
                s.DataSelecao
            })
            .ToListAsync();

        return Ok(selecoes);
    }

    [HttpPost("selecoes/{id:int}/aceitar")]
    public async Task<ActionResult> AceitarSelecao(int id)
    {
        var selecao = await _context.SelecoesTecnicos
            .Include(s => s.Historico)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (selecao is null)
        {
            return NotFound("Seleção não encontrada.");
        }

        try
        {
            selecao.Aceitar();
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensagem = "Solicitação aceita pelo técnico.",
                selecao.Id,
                selecao.SolicitacaoId,
                selecao.TecnicoId,
                Status = selecao.Status.ToString(),
                selecao.DataResposta
            });
        }
        catch (InvalidOperationException erro)
        {
            return BadRequest(erro.Message);
        }
    }

    [HttpPost("selecoes/{id:int}/recusar")]
    public async Task<ActionResult> RecusarSelecao(
        int id,
        RecusarSelecaoTecnicoDto dados)
    {
        var selecao = await _context.SelecoesTecnicos
            .Include(s => s.Historico)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (selecao is null)
        {
            return NotFound("Seleção não encontrada.");
        }

        try
        {
            selecao.Recusar(dados.Motivo);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensagem = "Solicitação recusada pelo técnico.",
                selecao.Id,
                selecao.SolicitacaoId,
                selecao.TecnicoId,
                Status = selecao.Status.ToString(),
                selecao.DataResposta
            });
        }
        catch (InvalidOperationException erro)
        {
            return BadRequest(erro.Message);
        }
    }

    [HttpGet("selecoes/{id:int}/historico")]
    public async Task<ActionResult> ListarHistorico(int id)
    {
        var selecaoExiste = await _context.SelecoesTecnicos
            .AnyAsync(s => s.Id == id);

        if (!selecaoExiste)
        {
            return NotFound("Seleção não encontrada.");
        }

        var historico = await _context.HistoricosSelecoesTecnicos
            .AsNoTracking()
            .Where(h => h.SelecaoTecnicoId == id)
            .OrderBy(h => h.DataRegistro)
            .Select(h => new
            {
                h.Id,
                h.SelecaoTecnicoId,
                Status = h.Status.ToString(),
                h.Observacao,
                h.DataRegistro
            })
            .ToListAsync();

        return Ok(historico);
    }

}