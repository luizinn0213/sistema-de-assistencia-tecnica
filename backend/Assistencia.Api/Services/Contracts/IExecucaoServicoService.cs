using Assistencia.Api.Dtos;
using Assistencia.Api.Models;

namespace Assistencia.Api.Services.Contracts;

public interface IExecucaoServicoService
{
    Task<ExecucaoServico> CriarAsync(
        CriarExecucaoServicoDto dados,
        CancellationToken cancellationToken = default);

    Task<ExecucaoServico> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ExecucaoServico> ObterPorOrcamentoAsync(
        int orcamentoId,
        CancellationToken cancellationToken = default);

    Task<ExecucaoServico> ObterPorSolicitacaoAsync(
        int solicitacaoId,
        CancellationToken cancellationToken = default);

    Task<ExecucaoServico> IniciarAsync(
        int id,
        IniciarExecucaoServicoDto dados,
        CancellationToken cancellationToken = default);

    Task<ExecucaoServico> ConcluirAsync(
        int id,
        ConcluirExecucaoServicoDto dados,
        CancellationToken cancellationToken = default);
}
