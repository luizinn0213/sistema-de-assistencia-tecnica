namespace Assistencia.Api.Models;

public class RefreshToken
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime DataCriacao { get; private set; }
    public DateTime DataExpiracao { get; private set; }
    public DateTime? DataRevogacao { get; private set; }
    public string? SubstituidoPorHash { get; private set; }

    public bool Ativo =>
        DataRevogacao is null && DataExpiracao > DateTime.UtcNow;

    private RefreshToken()
    {
    }

    public RefreshToken(
        int usuarioId,
        string tokenHash,
        DateTime dataExpiracao)
    {
        if (usuarioId <= 0)
        {
            throw new ArgumentException("O usuário é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("O hash do token é obrigatório.");
        }

        if (dataExpiracao <= DateTime.UtcNow)
        {
            throw new ArgumentException("A expiração do token deve estar no futuro.");
        }

        UsuarioId = usuarioId;
        TokenHash = tokenHash.Trim();
        DataCriacao = DateTime.UtcNow;
        DataExpiracao = dataExpiracao;
    }

    public void Revogar(string? substituidoPorHash = null)
    {
        if (DataRevogacao is not null)
        {
            return;
        }

        DataRevogacao = DateTime.UtcNow;
        SubstituidoPorHash = string.IsNullOrWhiteSpace(substituidoPorHash)
            ? null
            : substituidoPorHash.Trim();
    }
}
