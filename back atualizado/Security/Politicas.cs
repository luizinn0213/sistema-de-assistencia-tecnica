namespace Api.Security;

/// <summary>
/// Nomes das políticas de autorização registradas no Program.cs.
/// Usar constantes evita erro de digitação em <c>[Authorize(Policy = ...)]</c>.
/// </summary>
public static class Politicas
{
    /// <summary>Apenas perfil Administrador.</summary>
    public const string SomenteAdministrador = "SomenteAdministrador";

    /// <summary>Tecnico e Administrador (equipe interna).</summary>
    public const string EquipeTecnica = "EquipeTecnica";

    /// <summary>Apenas perfil Cliente.</summary>
    public const string SomenteCliente = "SomenteCliente";

    /// <summary>Política de rate limit aplicada a login/refresh/cadastro.</summary>
    public const string RateLimitAuth = "auth";
}
