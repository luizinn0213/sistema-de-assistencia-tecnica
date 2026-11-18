using Assistencia.Api.Models;

namespace Assistencia.Api.Repositories.Contracts;

public interface IMatchingRepository
{
    Task<List<Tecnico>> BuscarTecnicosElegiveisAsync(
        int especialidadeId,
        int? solicitacaoId = null,
        CancellationToken cancellationToken = default);
}
