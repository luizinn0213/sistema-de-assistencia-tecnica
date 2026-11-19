using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Assistencia.Api.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Assistencia.Api.Controllers;

[ApiController]
[Route("api/orcamentos")]
public class OrcamentosController : ControllerBase
{
    private readonly IOrcamentoService
        _orcamentoService;

    public OrcamentosController(
        IOrcamentoService orcamentoService)
    {
        _orcamentoService = orcamentoService;
    }

    [HttpPost]
    public async Task<ActionResult> Criar(
        CriarOrcamentoDto dados,
        CancellationToken cancellationToken)
    {
        var orcamento =
            await _orcamentoService.CriarAsync(
                dados,
                cancellationToken);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = orcamento.Id },
            CriarResposta(orcamento));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> ObterPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var orcamento =
            await _orcamentoService.ObterPorIdAsync(
                id,
                cancellationToken);

        return Ok(CriarResposta(orcamento));
    }

    [HttpGet("diagnostico/{diagnosticoId:int}")]
    public async Task<ActionResult> ObterPorDiagnostico(
        int diagnosticoId,
        CancellationToken cancellationToken)
    {
        var orcamento =
            await _orcamentoService
                .ObterPorDiagnosticoAsync(
                    diagnosticoId,
                    cancellationToken);

        return Ok(CriarResposta(orcamento));
    }

    [HttpPost("{id:int}/itens")]
    public async Task<ActionResult> AdicionarItem(
        int id,
        ItemOrcamentoDto dados,
        CancellationToken cancellationToken)
    {
        var orcamento =
            await _orcamentoService.AdicionarItemAsync(
                id,
                dados,
                cancellationToken);

        return Ok(CriarResposta(orcamento));
    }

    [HttpPut("{id:int}/itens/{itemId:int}")]
    public async Task<ActionResult> AlterarItem(
        int id,
        int itemId,
        ItemOrcamentoDto dados,
        CancellationToken cancellationToken)
    {
        var orcamento =
            await _orcamentoService.AlterarItemAsync(
                id,
                itemId,
                dados,
                cancellationToken);

        return Ok(CriarResposta(orcamento));
    }

    [HttpDelete("{id:int}/itens/{itemId:int}")]
    public async Task<ActionResult> RemoverItem(
        int id,
        int itemId,
        CancellationToken cancellationToken)
    {
        var orcamento =
            await _orcamentoService.RemoverItemAsync(
                id,
                itemId,
                cancellationToken);

        return Ok(CriarResposta(orcamento));
    }

    [HttpPost("{id:int}/enviar")]
    public async Task<ActionResult> Enviar(
        int id,
        CancellationToken cancellationToken)
    {
        var orcamento =
            await _orcamentoService.EnviarAsync(
                id,
                cancellationToken);

        return Ok(CriarResposta(orcamento));
    }

    [HttpPost("{id:int}/aprovar")]
    public async Task<ActionResult> Aprovar(
        int id,
        RespostaOrcamentoDto dados,
        CancellationToken cancellationToken)
    {
        var orcamento =
            await _orcamentoService.AprovarAsync(
                id,
                dados,
                cancellationToken);

        return Ok(CriarResposta(orcamento));
    }

    [HttpPost("{id:int}/rejeitar")]
    public async Task<ActionResult> Rejeitar(
        int id,
        RespostaOrcamentoDto dados,
        CancellationToken cancellationToken)
    {
        var orcamento =
            await _orcamentoService.RejeitarAsync(
                id,
                dados,
                cancellationToken);

        return Ok(CriarResposta(orcamento));
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<ActionResult> Cancelar(
        int id,
        RespostaOrcamentoDto dados,
        CancellationToken cancellationToken)
    {
        var orcamento =
            await _orcamentoService.CancelarAsync(
                id,
                dados,
                cancellationToken);

        return Ok(CriarResposta(orcamento));
    }

    private static object CriarResposta(
        Orcamento orcamento)
    {
        return new
        {
            orcamento.Id,
            orcamento.DiagnosticoId,

            SolicitacaoId =
                orcamento.Diagnostico?.SolicitacaoId,

            SolicitacaoNumero =
                orcamento.Diagnostico?
                    .Solicitacao?.Numero,

            Status = orcamento.Status.ToString(),
            DataCriacao = ComoUtc(
                orcamento.DataCriacao),

            DataEnvio = ComoUtc(
                orcamento.DataEnvio),

            DataResposta = ComoUtc(
                orcamento.DataResposta),
            orcamento.ValorTotal,

            Itens = orcamento.Itens
                .OrderBy(i => i.Id)
                .Select(i => new
                {
                    i.Id,
                    i.Descricao,
                    Tipo = i.Tipo.ToString(),
                    i.Quantidade,
                    i.ValorUnitario,
                    i.ValorTotal
                }),

            Historico = orcamento.Historico
                .OrderBy(h => h.DataRegistro)
                .Select(h => new
                {
                    h.Id,
                    Status = h.Status.ToString(),
                    h.Observacao,
                    DataRegistro = ComoUtc(
                        h.DataRegistro)
                })
        };
    }
    private static DateTime ComoUtc(
        DateTime data)
    {
        return DateTime.SpecifyKind(
            data,
            DateTimeKind.Utc);
    }

    private static DateTime? ComoUtc(
        DateTime? data)
    {
        return data.HasValue
            ? DateTime.SpecifyKind(
                data.Value,
                DateTimeKind.Utc)
            : null;
    }
}