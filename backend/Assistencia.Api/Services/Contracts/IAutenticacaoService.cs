using Assistencia.Api.Dtos;

namespace Assistencia.Api.Services.Contracts;

public interface IAutenticacaoService
{
    Task<AutenticacaoRespostaDto> LoginAsync(
        LoginDto dados,
        CancellationToken cancellationToken = default);

    Task<AutenticacaoRespostaDto> RenovarAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<SessaoUsuarioDto> ObterSessaoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task TrocarSenhaAsync(
        int usuarioId,
        TrocarSenhaDto dados,
        CancellationToken cancellationToken = default);
}
