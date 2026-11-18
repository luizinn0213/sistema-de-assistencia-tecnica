namespace Api;

// ============================================================
// PERFIS DE ACESSO
// ============================================================
/// <summary>
/// Perfis (papéis) do sistema. O valor numérico era gravado no banco
/// antigo; agora o nome do perfil é gravado como texto (ver AppDbContext).
/// </summary>
public enum TipoUsuario
{
    Cliente = 1,
    Tecnico = 2,
    Administrador = 3
}

/// <summary>
/// Nomes dos perfis usados nas claims do JWT (claim "role") e nas políticas
/// de autorização. Manter tudo aqui evita "strings mágicas" espalhadas.
/// </summary>
public static class Perfis
{
    public const string Cliente = nameof(TipoUsuario.Cliente);
    public const string Tecnico = nameof(TipoUsuario.Tecnico);
    public const string Administrador = nameof(TipoUsuario.Administrador);

    /// <summary>Converte o enum no nome do perfil (usado no token).</summary>
    public static string De(TipoUsuario tipo) => tipo switch
    {
        TipoUsuario.Administrador => Administrador,
        TipoUsuario.Tecnico => Tecnico,
        _ => Cliente
    };

    /// <summary>
    /// Converte o texto do token no enum, rejeitando valores inexistentes
    /// (ex.: "99" ou "admin").
    /// </summary>
    public static bool TentarConverter(string? valor, out TipoUsuario tipo) =>
        Enum.TryParse(valor, ignoreCase: true, out tipo) && Enum.IsDefined(tipo);
}

// ============================================================
// ENTIDADES
// ============================================================
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";

    /// <summary>Hash BCrypt — a senha em texto puro nunca é armazenada.</summary>
    public string SenhaHash { get; set; } = "";

    public string? Telefone { get; set; }
    public TipoUsuario Tipo { get; set; } = TipoUsuario.Cliente;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public Cliente? Cliente { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class Cliente
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    /// <summary>Somente dígitos (11 = CPF, 14 = CNPJ).</summary>
    public string CpfCnpj { get; set; } = "";

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public Endereco? Endereco { get; set; }
    public ICollection<Equipamento> Equipamentos { get; set; } = new List<Equipamento>();
}

public class Endereco
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Cep { get; set; } = "";
    public string Logradouro { get; set; } = "";
    public string Numero { get; set; } = "";
    public string Bairro { get; set; } = "";
    public string Cidade { get; set; } = "";
    public string Estado { get; set; } = "";
}

public class Equipamento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public string Tipo { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string NumeroSerie { get; set; } = "";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Refresh token de longa duração. Guardamos apenas o hash SHA-256 do token:
/// se o banco vazar, os tokens não podem ser reutilizados.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string TokenHash { get; set; } = "";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime ExpiraEm { get; set; }
    public DateTime? RevogadoEm { get; set; }

    /// <summary>Hash do token que substituiu este (rotação de refresh token).</summary>
    public string? SubstituidoPor { get; set; }

    public string? CriadoPorIp { get; set; }

    /// <summary>Propriedade calculada — não é coluna no banco.</summary>
    public bool Ativo => RevogadoEm is null && ExpiraEm > DateTime.UtcNow;
}
