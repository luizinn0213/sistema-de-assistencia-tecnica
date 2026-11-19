using Assistencia.Api.Models;

namespace Assistencia.Api.Repositories.Contracts;

public interface IExecucaoServicoRepository
{
    Task<ExecucaoServico?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ExecucaoServico?> ObterPorOrcamentoAsync(
        int orcamentoId,
        CancellationToken cancellationToken = default);

    Task<ExecucaoServico?> ObterPorSolicitacaoAsync(
        int solicitacaoId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteParaOrcamentoAsync(
        int orcamentoId,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        ExecucaoServico execucaoServico,
        CancellationToken cancellationToken = default);

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default);
}
