using Assistencia.Api.Dtos.Matching;

namespace Assistencia.Api.Services.Contracts;

public interface IMatchingService
{
    Task<List<TecnicoRankingDto>> GerarRankingAsync(
        int especialidadeId,
        string? cidade = null,
        int? solicitacaoId = null,
        CancellationToken cancellationToken = default);
}
