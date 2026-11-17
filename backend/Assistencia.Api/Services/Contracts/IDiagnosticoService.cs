using Assistencia.Api.Dtos;
using Assistencia.Api.Models;

namespace Assistencia.Api.Services.Contracts;

public interface IDiagnosticoService
{
    Task<Diagnostico> CriarAsync(
        CriarDiagnosticoDto dados,
        CancellationToken cancellationToken = default);

    Task<Diagnostico> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Diagnostico> ObterPorSolicitacaoAsync(
        int solicitacaoId,
        CancellationToken cancellationToken = default);

    Task<Diagnostico> AtualizarAsync(
        int id,
        AtualizarDiagnosticoDto dados,
        CancellationToken cancellationToken = default);

    Task<Diagnostico> ConcluirAsync(
        int id,
        CancellationToken cancellationToken = default);
}