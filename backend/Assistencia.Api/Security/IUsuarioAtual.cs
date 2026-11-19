namespace Assistencia.Api.Security;

public interface IUsuarioAtual
{
    int? UsuarioId { get; }
    int? ClienteId { get; }
    string? Perfil { get; }
}
