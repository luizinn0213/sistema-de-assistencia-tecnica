using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Assistencia.Api.Services.Contracts;

namespace Assistencia.Api.Services;

public class AvaliacaoService : IAvaliacaoService
{
    private readonly IAvaliacaoRepository _avaliacaoRepository;
    private readonly IExecucaoServicoRepository _execucaoRepository;

    public AvaliacaoService(
        IAvaliacaoRepository avaliacaoRepository,
        IExecucaoServicoRepository execucaoRepository)
    {
        _avaliacaoRepository = avaliacaoRepository;
        _execucaoRepository = execucaoRepository;
    }

    public async Task<Avaliacao> CriarAsync(
        CriarAvaliacaoDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);

        if (dados.ExecucaoServicoId <= 0)
        {
            throw new ArgumentException(
                "A execução é obrigatória.");
        }

        var existe = await _avaliacaoRepository
            .ExisteParaExecucaoAsync(
                dados.ExecucaoServicoId,
                cancellationToken);

        if (existe)
        {
            throw new InvalidOperationException(
                "Essa execução já possui uma avaliação.");
        }

        var execucao = await _execucaoRepository.ObterPorIdAsync(
            dados.ExecucaoServicoId,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "Execução não encontrada.");

        var solicitacao = execucao.Orcamento?
            .Diagnostico?.Solicitacao
            ?? throw new InvalidOperationException(
                "Não foi possível identificar a solicitação da execução.");

        if (solicitacao.ClienteId != dados.ClienteId)
        {
            throw new InvalidOperationException(
                "Somente o cliente da solicitação pode avaliar o serviço.");
        }

        var avaliacao = new Avaliacao(
            execucao,
            dados.ClienteId,
            dados.Nota,
            dados.Comentario);

        await _avaliacaoRepository.AdicionarAsync(
            avaliacao,
            cancellationToken);

        await _avaliacaoRepository.SalvarAlteracoesAsync(
            cancellationToken);

        return avaliacao;
    }

    public async Task<Avaliacao> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "A avaliação é obrigatória.");
        }

        return await _avaliacaoRepository.ObterPorIdAsync(
            id,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "Avaliação não encontrada.");
    }

    public async Task<Avaliacao> ObterPorExecucaoAsync(
        int execucaoServicoId,
        CancellationToken cancellationToken = default)
    {
        if (execucaoServicoId <= 0)
        {
            throw new ArgumentException(
                "A execução é obrigatória.");
        }

        return await _avaliacaoRepository.ObterPorExecucaoAsync(
            execucaoServicoId,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "Nenhuma avaliação foi encontrada para essa execução.");
    }
}
