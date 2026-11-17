using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Assistencia.Api.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/diagnosticos")]
public class DiagnosticosController : ControllerBase
{
    private readonly IDiagnosticoService
        _diagnosticoService;

    public DiagnosticosController(
        IDiagnosticoService diagnosticoService)
    {
        _diagnosticoService = diagnosticoService;
    }

    [HttpPost]
    public async Task<ActionResult> Criar(
        CriarDiagnosticoDto dados,
        CancellationToken cancellationToken)
    {
        var diagnostico =
            await _diagnosticoService.CriarAsync(
                dados,
                cancellationToken);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = diagnostico.Id },
            CriarResposta(diagnostico));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> ObterPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var diagnostico =
            await _diagnosticoService.ObterPorIdAsync(
                id,
                cancellationToken);

        return Ok(CriarResposta(diagnostico));
    }

    [HttpGet("solicitacao/{solicitacaoId:int}")]
    public async Task<ActionResult> ObterPorSolicitacao(
        int solicitacaoId,
        CancellationToken cancellationToken)
    {
        var diagnostico =
            await _diagnosticoService
                .ObterPorSolicitacaoAsync(
                    solicitacaoId,
                    cancellationToken);

        return Ok(CriarResposta(diagnostico));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Atualizar(
        int id,
        AtualizarDiagnosticoDto dados,
        CancellationToken cancellationToken)
    {
        var diagnostico =
            await _diagnosticoService.AtualizarAsync(
                id,
                dados,
                cancellationToken);

        return Ok(CriarResposta(diagnostico));
    }

    [HttpPost("{id:int}/concluir")]
    public async Task<ActionResult> Concluir(
        int id,
        CancellationToken cancellationToken)
    {
        var diagnostico =
            await _diagnosticoService.ConcluirAsync(
                id,
                cancellationToken);

        return Ok(CriarResposta(diagnostico));
    }

    private static object CriarResposta(
        Diagnostico diagnostico)
    {
        return new
        {
            diagnostico.Id,
            diagnostico.SolicitacaoId,

            SolicitacaoNumero =
                diagnostico.Solicitacao?.Numero,

            diagnostico.TecnicoId,

            Tecnico =
                diagnostico.Tecnico?.NomeExibicao,

            diagnostico.DescricaoDefeito,
            diagnostico.SolucaoRecomendada,
            Status = diagnostico.Status.ToString(),
            diagnostico.DataCriacao,
            diagnostico.DataConclusao
        };
    }
}