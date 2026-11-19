using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Assistencia.Api.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/execucoes")]
public class ExecucoesController : ControllerBase
{
    private readonly IExecucaoServicoService _execucaoService;

    public ExecucoesController(
        IExecucaoServicoService execucaoService)
    {
        _execucaoService = execucaoService;
    }

    [HttpPost]
    public async Task<ActionResult> Criar(
        CriarExecucaoServicoDto dados,
        CancellationToken cancellationToken)
    {
        var execucao = await _execucaoService.CriarAsync(
            dados,
            cancellationToken);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = execucao.Id },
            CriarResposta(execucao));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> ObterPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var execucao = await _execucaoService.ObterPorIdAsync(
            id,
            cancellationToken);
        return Ok(CriarResposta(execucao));
    }

    [HttpGet("orcamento/{orcamentoId:int}")]
    public async Task<ActionResult> ObterPorOrcamento(
        int orcamentoId,
        CancellationToken cancellationToken)
    {
        var execucao = await _execucaoService.ObterPorOrcamentoAsync(
            orcamentoId,
            cancellationToken);
        return Ok(CriarResposta(execucao));
    }

    [HttpGet("solicitacao/{solicitacaoId:int}")]
    public async Task<ActionResult> ObterPorSolicitacao(
        int solicitacaoId,
        CancellationToken cancellationToken)
    {
        var execucao = await _execucaoService.ObterPorSolicitacaoAsync(
            solicitacaoId,
            cancellationToken);
        return Ok(CriarResposta(execucao));
    }

    [HttpPost("{id:int}/iniciar")]
    public async Task<ActionResult> Iniciar(
        int id,
        IniciarExecucaoServicoDto dados,
        CancellationToken cancellationToken)
    {
        var execucao = await _execucaoService.IniciarAsync(
            id,
            dados,
            cancellationToken);
        return Ok(CriarResposta(execucao));
    }

    [HttpPost("{id:int}/concluir")]
    public async Task<ActionResult> Concluir(
        int id,
        ConcluirExecucaoServicoDto dados,
        CancellationToken cancellationToken)
    {
        var execucao = await _execucaoService.ConcluirAsync(
            id,
            dados,
            cancellationToken);
        return Ok(CriarResposta(execucao));
    }

    private static object CriarResposta(ExecucaoServico execucao)
    {
        var diagnostico = execucao.Orcamento?.Diagnostico;

        return new
        {
            execucao.Id,
            execucao.OrcamentoId,
            SolicitacaoId = diagnostico?.SolicitacaoId,
            SolicitacaoNumero = diagnostico?.Solicitacao?.Numero,
            TecnicoId = diagnostico?.TecnicoId,
            Tecnico = diagnostico?.Tecnico?.NomeExibicao,
            Status = execucao.Status.ToString(),
            DataCriacao = ComoUtc(execucao.DataCriacao),
            DataInicio = ComoUtc(execucao.DataInicio),
            DataConclusao = ComoUtc(execucao.DataConclusao),
            execucao.ObservacaoInicio,
            execucao.ObservacaoFinal,
            Historico = execucao.Historico
                .OrderBy(h => h.DataRegistro)
                .Select(h => new
                {
                    h.Id,
                    Status = h.Status.ToString(),
                    h.Observacao,
                    DataRegistro = ComoUtc(h.DataRegistro)
                })
        };
    }

    private static DateTime ComoUtc(DateTime data) =>
        DateTime.SpecifyKind(data, DateTimeKind.Utc);

    private static DateTime? ComoUtc(DateTime? data) =>
        data.HasValue
            ? DateTime.SpecifyKind(data.Value, DateTimeKind.Utc)
            : null;
}
