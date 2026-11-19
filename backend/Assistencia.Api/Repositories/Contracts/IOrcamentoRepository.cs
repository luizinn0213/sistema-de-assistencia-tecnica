using Assistencia.Api.Models;

namespace Assistencia.Api.Repositories.Contracts;

public interface IOrcamentoRepository
{
    Task<Orcamento?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Orcamento?> ObterPorDiagnosticoAsync(
        int diagnosticoId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteParaDiagnosticoAsync(
        int diagnosticoId,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        Orcamento orcamento,
        CancellationToken cancellationToken = default);

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default);
}