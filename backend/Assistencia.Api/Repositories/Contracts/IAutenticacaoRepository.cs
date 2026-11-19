using Assistencia.Api.Models;

namespace Assistencia.Api.Repositories.Contracts;

public interface IAutenticacaoRepository
{
    Task<Usuario?> ObterUsuarioPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<Usuario?> ObterUsuarioPorIdAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<int?> ObterClienteIdAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> ObterRefreshTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task AdicionarRefreshTokenAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);

    Task RevogarTokensAtivosAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default);
}
