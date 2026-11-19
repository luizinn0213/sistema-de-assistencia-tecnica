using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Assistencia.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Assistencia.Api.Security;

public class TokenService : ITokenService
{
    private readonly JwtOptions _opcoes;
    private readonly SigningCredentials _credenciais;

    public TokenService(IOptions<JwtOptions> opcoes)
    {
        _opcoes = opcoes.Value;

        if (Encoding.UTF8.GetByteCount(_opcoes.Chave) < 32)
        {
            throw new InvalidOperationException(
                "A chave JWT precisa possuir pelo menos 32 bytes.");
        }

        var chave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_opcoes.Chave));

        _credenciais = new SigningCredentials(
            chave,
            SecurityAlgorithms.HmacSha256);
    }

    public (string Token, DateTime ExpiraEm) GerarAccessToken(
        Usuario usuario,
        int? clienteId)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        var agora = DateTime.UtcNow;
        var expiraEm = agora.AddMinutes(_opcoes.ExpiraMinutos);

        var claims = new List<Claim>
        {
            new(ClaimsJwt.UsuarioId, usuario.Id.ToString()),
            new(ClaimsJwt.Nome, usuario.Nome),
            new(ClaimsJwt.Email, usuario.Email),
            new(ClaimsJwt.Perfil, usuario.Tipo.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        if (clienteId.HasValue)
        {
            claims.Add(new Claim(
                ClaimsJwt.ClienteId,
                clienteId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _opcoes.Issuer,
            audience: _opcoes.Audience,
            claims: claims,
            notBefore: agora,
            expires: expiraEm,
            signingCredentials: _credenciais);

        return (
            new JwtSecurityTokenHandler().WriteToken(token),
            expiraEm);
    }

    public (RefreshToken Registro, string Token) GerarRefreshToken(
        int usuarioId)
    {
        var token = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32))
            .ToLowerInvariant();

        var registro = new RefreshToken(
            usuarioId,
            CalcularHash(token),
            DateTime.UtcNow.AddDays(_opcoes.RefreshDias));

        return (registro, token);
    }

    public string CalcularHash(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("O token é obrigatório.");
        }

        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(token)))
            .ToLowerInvariant();
    }
}
