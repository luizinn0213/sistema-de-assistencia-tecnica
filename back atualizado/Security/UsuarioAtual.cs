using System.Security.Claims;

namespace Api.Security;

/// <summary>
/// Acesso ao usuário autenticado, obtido exclusivamente das claims do JWT.
/// É o que permite "identificar o usuário automaticamente": nenhum endpoint
/// confia em id de cliente vindo da URL ou do corpo da requisição.
/// </summary>
public interface IUsuarioAtual
{
    bool Autenticado { get; }
    int? Id { get; }
    int? ClienteId { get; }
    string? Nome { get; }
    string? Email { get; }
    TipoUsuario? Tipo { get; }
    bool EhAdministrador { get; }
    bool EhEquipe { get; }
}

public sealed class UsuarioAtual : IUsuarioAtual
{
    private readonly IHttpContextAccessor _acessoHttp;

    public UsuarioAtual(IHttpContextAccessor acessoHttp) => _acessoHttp = acessoHttp;

    private ClaimsPrincipal? Principal => _acessoHttp.HttpContext?.User;

    public bool Autenticado => Principal?.Identity?.IsAuthenticated == true;

    public int? Id => LerInteiro(ClaimsCustomizados.Id);

    public int? ClienteId => LerInteiro(ClaimsCustomizados.ClienteId);

    public string? Nome => Ler(ClaimsCustomizados.Nome);

    public string? Email => Ler(ClaimsCustomizados.Email);

    public TipoUsuario? Tipo =>
        Perfis.TentarConverter(Ler(ClaimsCustomizados.Tipo), out var tipo) ? tipo : null;

    public bool EhAdministrador => Tipo == TipoUsuario.Administrador;

    public bool EhEquipe => Tipo is TipoUsuario.Tecnico or TipoUsuario.Administrador;

    private string? Ler(string tipo) => Principal?.FindFirst(tipo)?.Value;

    private int? LerInteiro(string tipo) => int.TryParse(Ler(tipo), out var valor) ? valor : null;
}
