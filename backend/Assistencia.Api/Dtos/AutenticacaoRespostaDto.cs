namespace Assistencia.Api.Dtos;

public class AutenticacaoRespostaDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public DateTime AccessTokenExpiraEm { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiraEm { get; init; }
    public SessaoUsuarioDto Usuario { get; init; } = new();
}

public class SessaoUsuarioDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public int? ClienteId { get; init; }
}
