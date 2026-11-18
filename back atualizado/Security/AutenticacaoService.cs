using Microsoft.EntityFrameworkCore;

namespace Api.Security;

/// <summary>
/// Centraliza a emissão/renovação de tokens para que login, cadastro e
/// refresh devolvam exatamente o mesmo formato de resposta.
/// </summary>
public interface IAutenticacaoService
{
    Task<TokenRespostaDto> EmitirTokensAsync(Usuario usuario, string? ip, CancellationToken ct = default);

    /// <summary>Rotaciona o refresh token (revoga o antigo e emite um novo par).</summary>
    Task<TokenRespostaDto?> RenovarAsync(string refreshToken, string? ip, CancellationToken ct = default);

    Task RevogarTodosAsync(int usuarioId, CancellationToken ct = default);

    Task<int?> ObterClienteIdAsync(int usuarioId, CancellationToken ct = default);
}

public sealed class AutenticacaoService : IAutenticacaoService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokens;

    public AutenticacaoService(AppDbContext db, ITokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    public async Task<TokenRespostaDto> EmitirTokensAsync(Usuario usuario, string? ip, CancellationToken ct = default)
    {
        var clienteId = await ObterClienteIdAsync(usuario.Id, ct);

        var (accessToken, expiraEm) = _tokens.GerarAccessToken(usuario, clienteId);
        var (registro, refreshToken) = _tokens.GerarRefreshToken(usuario.Id, ip);

        _db.RefreshTokens.Add(registro);
        await _db.SaveChangesAsync(ct);

        var segundos = (int)Math.Max(0, Math.Round((expiraEm - DateTime.UtcNow).TotalSeconds));

        return new TokenRespostaDto(
            accessToken,
            "Bearer",
            segundos,
            refreshToken,
            registro.ExpiraEm,
            new UsuarioRespostaDto(
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Telefone,
                Perfis.De(usuario.Tipo),
                clienteId));
    }

    public async Task<TokenRespostaDto?> RenovarAsync(string refreshToken, string? ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var hash = _tokens.Hash(refreshToken.Trim());

        var atual = await _db.RefreshTokens
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        // Token inexistente, já usado (rotacionado), revogado ou vencido.
        if (atual is null || !atual.Ativo || !atual.Usuario.Ativo)
            return null;

        var resposta = await EmitirTokensAsync(atual.Usuario, ip, ct);

        atual.RevogadoEm = DateTime.UtcNow;
        atual.SubstituidoPor = _tokens.Hash(resposta.RefreshToken);
        await _db.SaveChangesAsync(ct);

        return resposta;
    }

    public async Task RevogarTodosAsync(int usuarioId, CancellationToken ct = default)
    {
        var ativos = await _db.RefreshTokens
            .Where(t => t.UsuarioId == usuarioId && t.RevogadoEm == null)
            .ToListAsync(ct);

        if (ativos.Count == 0)
            return;

        var agora = DateTime.UtcNow;
        foreach (var token in ativos)
            token.RevogadoEm = agora;

        await _db.SaveChangesAsync(ct);
    }

    public async Task<int?> ObterClienteIdAsync(int usuarioId, CancellationToken ct = default) =>
        await _db.Clientes
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(ct);
}
