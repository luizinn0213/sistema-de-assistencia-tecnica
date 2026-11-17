using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Assistencia.Api.Services.Contracts;

namespace Assistencia.Api.Services;

public class DiagnosticoService : IDiagnosticoService
{
    private readonly IDiagnosticoRepository
        _diagnosticoRepository;

    private readonly ISelecaoTecnicoRepository
        _selecaoRepository;

    public DiagnosticoService(
        IDiagnosticoRepository diagnosticoRepository,
        ISelecaoTecnicoRepository selecaoRepository)
    {
        _diagnosticoRepository =
            diagnosticoRepository;

        _selecaoRepository =
            selecaoRepository;
    }

    public async Task<Diagnostico> CriarAsync(
        CriarDiagnosticoDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);

        if (dados.SolicitacaoId <= 0)
        {
            throw new ArgumentException(
                "A solicitação é obrigatória.");
        }

        var diagnosticoExistente =
            await _diagnosticoRepository
                .ExisteParaSolicitacaoAsync(
                    dados.SolicitacaoId,
                    cancellationToken);

        if (diagnosticoExistente)
        {
            throw new InvalidOperationException(
                "Essa solicitação já possui um diagnóstico.");
        }

        var selecao = await _selecaoRepository
            .ObterAceitaPorSolicitacaoAsync(
                dados.SolicitacaoId,
                cancellationToken);

        if (selecao is null)
        {
            throw new InvalidOperationException(
                "A solicitação não possui uma seleção aceita.");
        }

        var diagnostico = new Diagnostico(
            selecao,
            dados.DescricaoDefeito);

        await _diagnosticoRepository.AdicionarAsync(
            diagnostico,
            cancellationToken);

        await _diagnosticoRepository.SalvarAlteracoesAsync(
            cancellationToken);

        return diagnostico;
    }

    public async Task<Diagnostico> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "O diagnóstico é obrigatório.");
        }

        return await _diagnosticoRepository
            .ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException(
                "Diagnóstico não encontrado.");
    }

    public async Task<Diagnostico>
        ObterPorSolicitacaoAsync(
            int solicitacaoId,
            CancellationToken cancellationToken = default)
    {
        if (solicitacaoId <= 0)
        {
            throw new ArgumentException(
                "A solicitação é obrigatória.");
        }

        return await _diagnosticoRepository
            .ObterPorSolicitacaoAsync(
                solicitacaoId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Nenhum diagnóstico foi encontrado " +
                "para essa solicitação.");
    }

    public async Task<Diagnostico> AtualizarAsync(
        int id,
        AtualizarDiagnosticoDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);

        var diagnostico = await ObterPorIdAsync(
            id,
            cancellationToken);

        diagnostico.AlterarInformacoes(
            dados.DescricaoDefeito,
            dados.SolucaoRecomendada);

        await _diagnosticoRepository.SalvarAlteracoesAsync(
            cancellationToken);

        return diagnostico;
    }

    public async Task<Diagnostico> ConcluirAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var diagnostico = await ObterPorIdAsync(
            id,
            cancellationToken);

        diagnostico.Concluir();

        await _diagnosticoRepository.SalvarAlteracoesAsync(
            cancellationToken);

        return diagnostico;
    }
}