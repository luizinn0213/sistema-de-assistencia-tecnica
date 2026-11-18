using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Api.Security;

public interface ITokenService
{
    /// <summary>Gera o access token (JWT) assinado com HMAC-SHA256.</summary>
    (string Token, DateTime ExpiraEm) GerarAccessToken(Usuario usuario, int? clienteId);

    /// <summary>Gera um refresh token aleatório e a entidade já com o hash.</summary>
    (RefreshToken Registro, string Token) GerarRefreshToken(int usuarioId, string? ip);

    /// <summary>Hash SHA-256 (hex) usado para guardar/localizar refresh tokens.</summary>
    string Hash(string token);
}

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _opcoes;
    private readonly SigningCredentials _credenciais;
    private readonly JsonWebTokenHandler _handler = new();

    public TokenService(IOptions<JwtOptions> opcoes)
    {
        _opcoes = opcoes.Value;
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opcoes.Chave));
        _credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
    }

    public (string Token, DateTime ExpiraEm) GerarAccessToken(Usuario usuario, int? clienteId)
    {
        var agora = DateTime.UtcNow;
        var expiraEm = agora.Add(_opcoes.ExpiracaoAccessToken);

        var claims = new Dictionary<string, object>
        {
            [ClaimsCustomizados.Id] = usuario.Id.ToString(),
            [ClaimsCustomizados.Nome] = usuario.Nome,
            [ClaimsCustomizados.Email] = usuario.Email,
            [ClaimsCustomizados.Perfil] = Perfis.De(usuario.Tipo),
            [ClaimsCustomizados.Tipo] = usuario.Tipo.ToString(),
            [ClaimsCustomizados.TokenId] = Guid.NewGuid().ToString("N")
        };

        // O vínculo com o cliente vai no token: os endpoints de cliente não
        // precisam (e não devem) confiar em id vindo da URL/body.
        if (clienteId is int idCliente)
            claims[ClaimsCustomizados.ClienteId] = idCliente.ToString();

        var descritor = new SecurityTokenDescriptor
        {
            Issuer = _opcoes.Issuer,
            Audience = _opcoes.Audience,
            IssuedAt = agora,
            NotBefore = agora,
            Expires = expiraEm,
            Claims = claims,
            SigningCredentials = _credenciais
        };

        return (_handler.CreateToken(descritor), expiraEm);
    }

    public (RefreshToken Registro, string Token) GerarRefreshToken(int usuarioId, string? ip)
    {
        // 32 bytes aleatórios criptograficamente seguros → 64 caracteres hex.
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

        var registro = new RefreshToken
        {
            UsuarioId = usuarioId,
            TokenHash = Hash(token),
            ExpiraEm = DateTime.UtcNow.Add(_opcoes.ExpiracaoRefreshToken),
            CriadoPorIp = ip
        };

        return (registro, token);
    }

    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}
