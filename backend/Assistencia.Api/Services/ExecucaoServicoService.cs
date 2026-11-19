using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Assistencia.Api.Services.Contracts;

namespace Assistencia.Api.Services;

public class ExecucaoServicoService : IExecucaoServicoService
{
    private readonly IExecucaoServicoRepository _execucaoRepository;
    private readonly IOrcamentoRepository _orcamentoRepository;

    public ExecucaoServicoService(
        IExecucaoServicoRepository execucaoRepository,
        IOrcamentoRepository orcamentoRepository)
    {
        _execucaoRepository = execucaoRepository;
        _orcamentoRepository = orcamentoRepository;
    }

    public async Task<ExecucaoServico> CriarAsync(
        CriarExecucaoServicoDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);

        if (dados.OrcamentoId <= 0)
        {
            throw new ArgumentException(
                "O orçamento é obrigatório.");
        }

        var existe = await _execucaoRepository
            .ExisteParaOrcamentoAsync(
                dados.OrcamentoId,
                cancellationToken);

        if (existe)
        {
            throw new InvalidOperationException(
                "Esse orçamento já possui uma execução.");
        }

        var orcamento = await _orcamentoRepository
            .ObterPorIdAsync(
                dados.OrcamentoId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Orçamento não encontrado.");

        var execucao = new ExecucaoServico(orcamento);

        await _execucaoRepository.AdicionarAsync(
            execucao,
            cancellationToken);

        await SalvarAsync(cancellationToken);
        return execucao;
    }

    public async Task<ExecucaoServico> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        ValidarId(id);
        return await _execucaoRepository.ObterPorIdAsync(
            id,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "Execução não encontrada.");
    }

    public async Task<ExecucaoServico> ObterPorOrcamentoAsync(
        int orcamentoId,
        CancellationToken cancellationToken = default)
    {
        ValidarId(orcamentoId);
        return await _execucaoRepository.ObterPorOrcamentoAsync(
            orcamentoId,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "Nenhuma execução foi encontrada para esse orçamento.");
    }

    public async Task<ExecucaoServico> ObterPorSolicitacaoAsync(
        int solicitacaoId,
        CancellationToken cancellationToken = default)
    {
        ValidarId(solicitacaoId);
        return await _execucaoRepository.ObterPorSolicitacaoAsync(
            solicitacaoId,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "Nenhuma execução foi encontrada para essa solicitação.");
    }

    public async Task<ExecucaoServico> IniciarAsync(
        int id,
        IniciarExecucaoServicoDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);
        var execucao = await ObterPorIdAsync(id, cancellationToken);
        execucao.Iniciar(dados.Observacao);
        await SalvarAsync(cancellationToken);
        return execucao;
    }

    public async Task<ExecucaoServico> ConcluirAsync(
        int id,
        ConcluirExecucaoServicoDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);
        var execucao = await ObterPorIdAsync(id, cancellationToken);
        execucao.Concluir(dados.ObservacaoFinal);
        await SalvarAsync(cancellationToken);
        return execucao;
    }

    private static void ValidarId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "O identificador é obrigatório.");
        }
    }

    private Task SalvarAsync(CancellationToken cancellationToken)
    {
        return _execucaoRepository
            .SalvarAlteracoesAsync(cancellationToken);
    }
}
