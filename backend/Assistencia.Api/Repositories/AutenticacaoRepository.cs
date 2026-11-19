using Assistencia.Api.Data;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Repositories;

public class AutenticacaoRepository : IAutenticacaoRepository
{
    private readonly AppDbContext _context;

    public AutenticacaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Usuario?> ObterUsuarioPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return _context.Usuarios.FirstOrDefaultAsync(
            u => u.Email == email,
            cancellationToken);
    }

    public Task<Usuario?> ObterUsuarioPorIdAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        return _context.Usuarios.FirstOrDefaultAsync(
            u => u.Id == usuarioId,
            cancellationToken);
    }

    public Task<int?> ObterClienteIdAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        return _context.Clientes
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<RefreshToken?> ObterRefreshTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _context.RefreshTokens
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task AdicionarRefreshTokenAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }

    public async Task RevogarTokensAtivosAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(t =>
                t.UsuarioId == usuarioId &&
                t.DataRevogacao == null &&
                t.DataExpiracao > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revogar();
        }
    }

    public Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
