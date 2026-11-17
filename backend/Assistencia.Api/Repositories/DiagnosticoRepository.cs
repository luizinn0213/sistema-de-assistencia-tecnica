using Assistencia.Api.Data;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Repositories;

public class DiagnosticoRepository : IDiagnosticoRepository
{
    private readonly AppDbContext _context;

    public DiagnosticoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Diagnostico?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Diagnosticos
            .Include(d => d.Solicitacao)
                .ThenInclude(s => s!.Equipamento)
            .Include(d => d.Tecnico)
            .FirstOrDefaultAsync(
                d => d.Id == id,
                cancellationToken);
    }

    public Task<Diagnostico?> ObterPorSolicitacaoAsync(
        int solicitacaoId,
        CancellationToken cancellationToken = default)
    {
        return _context.Diagnosticos
            .Include(d => d.Solicitacao)
                .ThenInclude(s => s!.Equipamento)
            .Include(d => d.Tecnico)
            .FirstOrDefaultAsync(
                d => d.SolicitacaoId == solicitacaoId,
                cancellationToken);
    }

    public Task<bool> ExisteParaSolicitacaoAsync(
        int solicitacaoId,
        CancellationToken cancellationToken = default)
    {
        return _context.Diagnosticos.AnyAsync(
            d => d.SolicitacaoId == solicitacaoId,
            cancellationToken);
    }

    public async Task AdicionarAsync(
        Diagnostico diagnostico,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(diagnostico);

        await _context.Diagnosticos.AddAsync(
            diagnostico,
            cancellationToken);
    }

    public Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}