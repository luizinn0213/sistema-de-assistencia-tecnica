using Assistencia.Api.Data;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Repositories;

public class ExecucaoServicoRepository : IExecucaoServicoRepository
{
    private readonly AppDbContext _context;

    public ExecucaoServicoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<ExecucaoServico?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return CriarConsultaCompleta().FirstOrDefaultAsync(
            e => e.Id == id,
            cancellationToken);
    }

    public Task<ExecucaoServico?> ObterPorOrcamentoAsync(
        int orcamentoId,
        CancellationToken cancellationToken = default)
    {
        return CriarConsultaCompleta().FirstOrDefaultAsync(
            e => e.OrcamentoId == orcamentoId,
            cancellationToken);
    }

    public Task<ExecucaoServico?> ObterPorSolicitacaoAsync(
        int solicitacaoId,
        CancellationToken cancellationToken = default)
    {
        return CriarConsultaCompleta().FirstOrDefaultAsync(
            e => e.Orcamento != null &&
                 e.Orcamento.Diagnostico != null &&
                 e.Orcamento.Diagnostico.SolicitacaoId == solicitacaoId,
            cancellationToken);
    }

    public Task<bool> ExisteParaOrcamentoAsync(
        int orcamentoId,
        CancellationToken cancellationToken = default)
    {
        return _context.ExecucoesServicos.AnyAsync(
            e => e.OrcamentoId == orcamentoId,
            cancellationToken);
    }

    public async Task AdicionarAsync(
        ExecucaoServico execucaoServico,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(execucaoServico);
        await _context.ExecucoesServicos.AddAsync(
            execucaoServico,
            cancellationToken);
    }

    public Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<ExecucaoServico> CriarConsultaCompleta()
    {
        return _context.ExecucoesServicos
            .Include(e => e.Historico)
            .Include(e => e.Orcamento)
                .ThenInclude(o => o!.Diagnostico)
                    .ThenInclude(d => d!.Solicitacao)
            .Include(e => e.Orcamento)
                .ThenInclude(o => o!.Diagnostico)
                    .ThenInclude(d => d!.Tecnico)
            .AsSplitQuery();
    }
}
