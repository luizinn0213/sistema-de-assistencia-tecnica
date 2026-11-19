using Assistencia.Api.Enums;

namespace Assistencia.Api.Security;

public static class PoliticasAutorizacao
{
    public const string SomenteCliente = "SomenteCliente";
    public const string EquipeTecnica = "EquipeTecnica";
    public const string SomenteAdministrador = "SomenteAdministrador";

    public static string Cliente => TipoUsuario.Cliente.ToString();
    public static string Tecnico => TipoUsuario.Tecnico.ToString();
    public static string Administrador => TipoUsuario.Administrador.ToString();
}
