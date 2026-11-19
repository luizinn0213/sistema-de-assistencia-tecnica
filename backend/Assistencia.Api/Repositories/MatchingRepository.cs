using Assistencia.Api.Data;
using Assistencia.Api.Enums;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Repositories;

public class MatchingRepository : IMatchingRepository
{
    private readonly AppDbContext _context;

    public MatchingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Tecnico>> BuscarTecnicosElegiveisAsync(
        int especialidadeId,
        int? solicitacaoId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tecnicos
            .Include(t => t.TecnicoEspecialidades)
                .ThenInclude(te => te.Especialidade)
            .AsNoTracking()
            .Where(t =>
                t.Ativo &&
                t.Disponivel &&
                t.TecnicoEspecialidades.Any(te =>
                    te.EspecialidadeId == especialidadeId &&
                    te.Especialidade != null &&
                    te.Especialidade.Ativa));

        if (solicitacaoId.HasValue && solicitacaoId.Value > 0)
        {
            var tecnicosRecusadosIds = _context.SelecoesTecnicos
                .Where(s =>
                    s.SolicitacaoId == solicitacaoId.Value &&
                    s.Status == StatusSelecaoTecnico.Recusada)
                .Select(s => s.TecnicoId);

            query = query.Where(t => !tecnicosRecusadosIds.Contains(t.Id));
        }

        return await query.ToListAsync(cancellationToken);
    }
}
