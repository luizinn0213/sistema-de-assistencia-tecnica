using Assistencia.Api.Data;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Repositories;

public class AvaliacaoRepository : IAvaliacaoRepository
{
    private readonly AppDbContext _context;

    public AvaliacaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Avaliacao?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return CriarConsultaCompleta().FirstOrDefaultAsync(
            a => a.Id == id,
            cancellationToken);
    }

    public Task<Avaliacao?> ObterPorExecucaoAsync(
        int execucaoServicoId,
        CancellationToken cancellationToken = default)
    {
        return CriarConsultaCompleta().FirstOrDefaultAsync(
            a => a.ExecucaoServicoId == execucaoServicoId,
            cancellationToken);
    }

    public Task<bool> ExisteParaExecucaoAsync(
        int execucaoServicoId,
        CancellationToken cancellationToken = default)
    {
        return _context.Avaliacoes.AnyAsync(
            a => a.ExecucaoServicoId == execucaoServicoId,
            cancellationToken);
    }

    public async Task AdicionarAsync(
        Avaliacao avaliacao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(avaliacao);
        await _context.Avaliacoes.AddAsync(
            avaliacao,
            cancellationToken);
    }

    public Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Avaliacao> CriarConsultaCompleta()
    {
        return _context.Avaliacoes
            .Include(a => a.Cliente)
                .ThenInclude(c => c!.Usuario)
            .Include(a => a.ExecucaoServico)
                .ThenInclude(e => e!.Orcamento)
                    .ThenInclude(o => o!.Diagnostico)
                        .ThenInclude(d => d!.Solicitacao)
            .AsSplitQuery();
    }
}
