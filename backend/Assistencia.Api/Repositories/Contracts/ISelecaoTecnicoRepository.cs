using Assistencia.Api.Models;

namespace Assistencia.Api.Repositories.Contracts;

public interface ISelecaoTecnicoRepository
{
    Task<SelecaoTecnico?> ObterAceitaPorSolicitacaoAsync(
        int solicitacaoId,
        CancellationToken cancellationToken = default);
}