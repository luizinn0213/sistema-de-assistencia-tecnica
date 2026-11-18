namespace Api.Security;

/// <summary>
/// Configuração da seção "Jwt" do appsettings.
/// A chave deve ser definida em produção via variável de ambiente
/// <c>Jwt__Chave</c> ou <c>dotnet user-secrets set "Jwt:Chave" "..."</c>.
/// </summary>
public sealed class JwtOptions
{
    public const string Secao = "Jwt";

    public string Issuer { get; set; } = "Api.Assistencia";
    public string Audience { get; set; } = "Api.Assistencia.Clientes";
    public string Chave { get; set; } = "";

    /// <summary>Duração do access token (curta).</summary>
    public int ExpiraMinutos { get; set; } = 60;

    /// <summary>Duração do refresh token (longa).</summary>
    public int RefreshExpiraDias { get; set; } = 7;

    public TimeSpan ExpiracaoAccessToken => TimeSpan.FromMinutes(Math.Clamp(ExpiraMinutos, 1, 1440));
    public TimeSpan ExpiracaoRefreshToken => TimeSpan.FromDays(Math.Clamp(RefreshExpiraDias, 1, 90));
}

/// <summary>
/// Nomes das claims usadas no JWT. Como o token é validado com
/// <c>MapInboundClaims = false</c>, os nomes abaixo chegam intactos ao
/// <c>ClaimsPrincipal</c> (sem o "http://schemas.microsoft.com/...").
/// </summary>
public static class ClaimsCustomizados
{
    /// <summary>Id do usuário (claim "sub").</summary>
    public const string Id = "sub";

    public const string Nome = "nome";
    public const string Email = "email";

    /// <summary>Perfil do usuário — neste nome o [Authorize(Roles = ...)] procura.</summary>
    public const string Perfil = "role";

    /// <summary>Nome do enum (Cliente/Tecnico/Administrador).</summary>
    public const string Tipo = "tipo";

    /// <summary>Id do cliente vinculado, quando o perfil for Cliente.</summary>
    public const string ClienteId = "clienteId";

    public const string TokenId = "jti";
}
