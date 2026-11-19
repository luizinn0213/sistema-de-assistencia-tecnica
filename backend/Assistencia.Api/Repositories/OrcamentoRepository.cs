using Assistencia.Api.Data;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Repositories;

public class OrcamentoRepository : IOrcamentoRepository
{
    private readonly AppDbContext _context;

    public OrcamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Orcamento?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return CriarConsultaCompleta()
            .FirstOrDefaultAsync(
                o => o.Id == id,
                cancellationToken);
    }

    public Task<Orcamento?> ObterPorDiagnosticoAsync(
        int diagnosticoId,
        CancellationToken cancellationToken = default)
    {
        return CriarConsultaCompleta()
            .FirstOrDefaultAsync(
                o => o.DiagnosticoId == diagnosticoId,
                cancellationToken);
    }

    public Task<bool> ExisteParaDiagnosticoAsync(
        int diagnosticoId,
        CancellationToken cancellationToken = default)
    {
        return _context.Orcamentos.AnyAsync(
            o => o.DiagnosticoId == diagnosticoId,
            cancellationToken);
    }

    public async Task AdicionarAsync(
        Orcamento orcamento,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orcamento);

        await _context.Orcamentos.AddAsync(
            orcamento,
            cancellationToken);
    }

    public Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(
            cancellationToken);
    }

    private IQueryable<Orcamento>
        CriarConsultaCompleta()
    {
        return _context.Orcamentos
            .Include(o => o.Itens)
            .Include(o => o.Historico)
            .Include(o => o.Diagnostico)
                .ThenInclude(d => d!.Solicitacao)
            .AsSplitQuery();
    }
}