using System.Security.Claims;

namespace Assistencia.Api.Security;

public class UsuarioAtual : IUsuarioAtual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioAtual(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UsuarioId => ObterInteiro(ClaimsJwt.UsuarioId);
    public int? ClienteId => ObterInteiro(ClaimsJwt.ClienteId);
    public string? Perfil => ObterClaim(ClaimsJwt.Perfil);

    private string? ObterClaim(string tipo)
    {
        return _httpContextAccessor.HttpContext?
            .User.FindFirstValue(tipo);
    }

    private int? ObterInteiro(string tipo)
    {
        return int.TryParse(ObterClaim(tipo), out var valor)
            ? valor
            : null;
    }
}
