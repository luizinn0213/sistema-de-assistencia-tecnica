using Assistencia.Api.Data;
using Assistencia.Api.Enums;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Repositories;

public class SelecaoTecnicoRepository
    : ISelecaoTecnicoRepository
{
    private readonly AppDbContext _context;

    public SelecaoTecnicoRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public Task<SelecaoTecnico?>
        ObterAceitaPorSolicitacaoAsync(
            int solicitacaoId,
            CancellationToken cancellationToken = default)
    {
        return _context.SelecoesTecnicos
            .Include(s => s.Solicitacao)
                .ThenInclude(s => s!.Equipamento)
            .Include(s => s.Tecnico)
            .Include(s => s.Especialidade)
            .FirstOrDefaultAsync(
                s =>
                    s.SolicitacaoId == solicitacaoId &&
                    s.Status == StatusSelecaoTecnico.Aceita,
                cancellationToken);
    }
}