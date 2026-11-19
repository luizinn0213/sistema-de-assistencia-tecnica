using Assistencia.Api.Models;

namespace Assistencia.Api.Security;

public interface ITokenService
{
    (string Token, DateTime ExpiraEm) GerarAccessToken(
        Usuario usuario,
        int? clienteId);

    (RefreshToken Registro, string Token) GerarRefreshToken(
        int usuarioId);

    string CalcularHash(string token);
}
