using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Assistencia.Api.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/avaliacoes")]
public class AvaliacoesController : ControllerBase
{
    private readonly IAvaliacaoService _avaliacaoService;

    public AvaliacoesController(IAvaliacaoService avaliacaoService)
    {
        _avaliacaoService = avaliacaoService;
    }

    [HttpPost]
    public async Task<ActionResult> Criar(
        CriarAvaliacaoDto dados,
        CancellationToken cancellationToken)
    {
        var avaliacao = await _avaliacaoService.CriarAsync(
            dados,
            cancellationToken);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = avaliacao.Id },
            CriarResposta(avaliacao));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> ObterPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var avaliacao = await _avaliacaoService.ObterPorIdAsync(
            id,
            cancellationToken);
        return Ok(CriarResposta(avaliacao));
    }

    [HttpGet("execucao/{execucaoServicoId:int}")]
    public async Task<ActionResult> ObterPorExecucao(
        int execucaoServicoId,
        CancellationToken cancellationToken)
    {
        var avaliacao = await _avaliacaoService.ObterPorExecucaoAsync(
            execucaoServicoId,
            cancellationToken);
        return Ok(CriarResposta(avaliacao));
    }

    private static object CriarResposta(Avaliacao avaliacao)
    {
        return new
        {
            avaliacao.Id,
            avaliacao.ExecucaoServicoId,
            avaliacao.ClienteId,
            Cliente = avaliacao.Cliente?.Usuario?.Nome,
            avaliacao.Nota,
            avaliacao.Comentario,
            DataAvaliacao = DateTime.SpecifyKind(
                avaliacao.DataAvaliacao,
                DateTimeKind.Utc)
        };
    }
}
