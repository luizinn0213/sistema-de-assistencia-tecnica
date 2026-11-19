using Assistencia.Api.Dtos;
using Assistencia.Api.Models;

namespace Assistencia.Api.Services.Contracts;

public interface IAvaliacaoService
{
    Task<Avaliacao> CriarAsync(
        CriarAvaliacaoDto dados,
        CancellationToken cancellationToken = default);

    Task<Avaliacao> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Avaliacao> ObterPorExecucaoAsync(
        int execucaoServicoId,
        CancellationToken cancellationToken = default);
}
