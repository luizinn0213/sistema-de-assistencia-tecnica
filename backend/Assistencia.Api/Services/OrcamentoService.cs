using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Assistencia.Api.Services.Contracts;

namespace Assistencia.Api.Services;

public class OrcamentoService : IOrcamentoService
{
    private readonly IOrcamentoRepository
        _orcamentoRepository;

    private readonly IDiagnosticoRepository
        _diagnosticoRepository;

    public OrcamentoService(
        IOrcamentoRepository orcamentoRepository,
        IDiagnosticoRepository diagnosticoRepository)
    {
        _orcamentoRepository = orcamentoRepository;
        _diagnosticoRepository = diagnosticoRepository;
    }

    public async Task<Orcamento> CriarAsync(
        CriarOrcamentoDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);

        if (dados.DiagnosticoId <= 0)
        {
            throw new ArgumentException(
                "O diagnóstico é obrigatório.");
        }

        var existe = await _orcamentoRepository
            .ExisteParaDiagnosticoAsync(
                dados.DiagnosticoId,
                cancellationToken);

        if (existe)
        {
            throw new InvalidOperationException(
                "Esse diagnóstico já possui um orçamento.");
        }

        var diagnostico = await _diagnosticoRepository
            .ObterPorIdAsync(
                dados.DiagnosticoId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Diagnóstico não encontrado.");

        var orcamento = new Orcamento(diagnostico);

        await _orcamentoRepository.AdicionarAsync(
            orcamento,
            cancellationToken);

        await _orcamentoRepository.SalvarAlteracoesAsync(
            cancellationToken);

        return orcamento;
    }

    public async Task<Orcamento> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "O orçamento é obrigatório.");
        }

        return await _orcamentoRepository.ObterPorIdAsync(
            id,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "Orçamento não encontrado.");
    }

    public async Task<Orcamento>
        ObterPorDiagnosticoAsync(
            int diagnosticoId,
            CancellationToken cancellationToken = default)
    {
        if (diagnosticoId <= 0)
        {
            throw new ArgumentException(
                "O diagnóstico é obrigatório.");
        }

        return await _orcamentoRepository
            .ObterPorDiagnosticoAsync(
                diagnosticoId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Nenhum orçamento foi encontrado " +
                "para esse diagnóstico.");
    }

    public async Task<Orcamento> AdicionarItemAsync(
        int orcamentoId,
        ItemOrcamentoDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);

        var orcamento = await ObterPorIdAsync(
            orcamentoId,
            cancellationToken);

        orcamento.AdicionarItem(
            dados.Descricao,
            dados.Tipo,
            dados.Quantidade,
            dados.ValorUnitario);

        await SalvarAsync(cancellationToken);

        return orcamento;
    }

    public async Task<Orcamento> AlterarItemAsync(
        int orcamentoId,
        int itemId,
        ItemOrcamentoDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);

        var orcamento = await ObterPorIdAsync(
            orcamentoId,
            cancellationToken);

        var item = ObterItem(orcamento, itemId);

        orcamento.AlterarItem(
            item,
            dados.Descricao,
            dados.Tipo,
            dados.Quantidade,
            dados.ValorUnitario);

        await SalvarAsync(cancellationToken);

        return orcamento;
    }

    public async Task<Orcamento> RemoverItemAsync(
        int orcamentoId,
        int itemId,
        CancellationToken cancellationToken = default)
    {
        var orcamento = await ObterPorIdAsync(
            orcamentoId,
            cancellationToken);

        var item = ObterItem(orcamento, itemId);

        orcamento.RemoverItem(item);

        await SalvarAsync(cancellationToken);

        return orcamento;
    }

    public async Task<Orcamento> EnviarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var orcamento = await ObterPorIdAsync(
            id,
            cancellationToken);

        orcamento.Enviar();

        await SalvarAsync(cancellationToken);

        return orcamento;
    }

    public async Task<Orcamento> AprovarAsync(
        int id,
        RespostaOrcamentoDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);

        var orcamento = await ObterPorIdAsync(
            id,
            cancellationToken);

        orcamento.Aprovar(dados.Observacao);

        await SalvarAsync(cancellationToken);

        return orcamento;
    }

    public async Task<Orcamento> RejeitarAsync(
        int id,
        RespostaOrcamentoDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);

        var orcamento = await ObterPorIdAsync(
            id,
            cancellationToken);

        orcamento.Rejeitar(dados.Observacao);

        await SalvarAsync(cancellationToken);

        return orcamento;
    }

    public async Task<Orcamento> CancelarAsync(
        int id,
        RespostaOrcamentoDto dados,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);

        var orcamento = await ObterPorIdAsync(
            id,
            cancellationToken);

        orcamento.Cancelar(dados.Observacao);

        await SalvarAsync(cancellationToken);

        return orcamento;
    }

    private static ItemOrcamento ObterItem(
        Orcamento orcamento,
        int itemId)
    {
        if (itemId <= 0)
        {
            throw new ArgumentException(
                "O item é obrigatório.");
        }

        return orcamento.Itens.FirstOrDefault(
            i => i.Id == itemId)
            ?? throw new KeyNotFoundException(
                "Item do orçamento não encontrado.");
    }

    private Task SalvarAsync(
        CancellationToken cancellationToken)
    {
        return _orcamentoRepository
            .SalvarAlteracoesAsync(cancellationToken);
    }
}