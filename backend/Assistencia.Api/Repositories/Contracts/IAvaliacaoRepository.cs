using Assistencia.Api.Models;

namespace Assistencia.Api.Repositories.Contracts;

public interface IAvaliacaoRepository
{
    Task<Avaliacao?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Avaliacao?> ObterPorExecucaoAsync(
        int execucaoServicoId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteParaExecucaoAsync(
        int execucaoServicoId,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        Avaliacao avaliacao,
        CancellationToken cancellationToken = default);

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default);
}
