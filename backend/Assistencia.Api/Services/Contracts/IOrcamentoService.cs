using Assistencia.Api.Dtos;
using Assistencia.Api.Models;

namespace Assistencia.Api.Services.Contracts;

public interface IOrcamentoService
{
    Task<Orcamento> CriarAsync(
        CriarOrcamentoDto dados,
        CancellationToken cancellationToken = default);

    Task<Orcamento> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Orcamento> ObterPorDiagnosticoAsync(
        int diagnosticoId,
        CancellationToken cancellationToken = default);

    Task<Orcamento> AdicionarItemAsync(
        int orcamentoId,
        ItemOrcamentoDto dados,
        CancellationToken cancellationToken = default);

    Task<Orcamento> AlterarItemAsync(
        int orcamentoId,
        int itemId,
        ItemOrcamentoDto dados,
        CancellationToken cancellationToken = default);

    Task<Orcamento> RemoverItemAsync(
        int orcamentoId,
        int itemId,
        CancellationToken cancellationToken = default);

    Task<Orcamento> EnviarAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Orcamento> AprovarAsync(
        int id,
        RespostaOrcamentoDto dados,
        CancellationToken cancellationToken = default);

    Task<Orcamento> RejeitarAsync(
        int id,
        RespostaOrcamentoDto dados,
        CancellationToken cancellationToken = default);

    Task<Orcamento> CancelarAsync(
        int id,
        RespostaOrcamentoDto dados,
        CancellationToken cancellationToken = default);
}