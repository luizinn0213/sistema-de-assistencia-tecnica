using Assistencia.Api.Models;

namespace Assistencia.Api.Repositories.Contracts;

public interface IDiagnosticoRepository
{
    Task<Diagnostico?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Diagnostico?> ObterPorSolicitacaoAsync(
        int solicitacaoId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteParaSolicitacaoAsync(
        int solicitacaoId,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        Diagnostico diagnostico,
        CancellationToken cancellationToken = default);

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default);
}