using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Assistencia.Api.Services;

namespace Assistencia.Api.Tests;

public class OrcamentoServiceTests
{
    [Fact]
    public async Task DeveCriarOrcamentoParaDiagnosticoConcluido()
    {
        var diagnostico = CriarDiagnosticoConcluido();

        var orcamentoRepository =
            new OrcamentoRepositoryFake();

        var diagnosticoRepository =
            new DiagnosticoRepositoryFake(diagnostico);

        var service = new OrcamentoService(
            orcamentoRepository,
            diagnosticoRepository);

        var resultado = await service.CriarAsync(
            new CriarOrcamentoDto
            {
                DiagnosticoId = 1
            });

        Assert.NotNull(resultado);
        Assert.Same(
            resultado,
            orcamentoRepository.OrcamentoAdicionado);

        Assert.True(
            orcamentoRepository.AlteracoesSalvas);
    }

    [Fact]
    public async Task NaoDeveCriarOrcamentoDuplicado()
    {
        var orcamentoRepository =
            new OrcamentoRepositoryFake
            {
                ExisteOrcamento = true
            };

        var diagnosticoRepository =
            new DiagnosticoRepositoryFake(
                CriarDiagnosticoConcluido());

        var service = new OrcamentoService(
            orcamentoRepository,
            diagnosticoRepository);

        var erro = await Assert.ThrowsAsync<
            InvalidOperationException>(
                () => service.CriarAsync(
                    new CriarOrcamentoDto
                    {
                        DiagnosticoId = 1
                    }));

        Assert.Equal(
            "Esse diagnóstico já possui um orçamento.",
            erro.Message);

        Assert.Null(
            orcamentoRepository.OrcamentoAdicionado);
    }

    [Fact]
    public async Task NaoDeveCriarSemDiagnostico()
    {
        var orcamentoRepository =
            new OrcamentoRepositoryFake();

        var diagnosticoRepository =
            new DiagnosticoRepositoryFake(null);

        var service = new OrcamentoService(
            orcamentoRepository,
            diagnosticoRepository);

        var erro = await Assert.ThrowsAsync<
            KeyNotFoundException>(
                () => service.CriarAsync(
                    new CriarOrcamentoDto
                    {
                        DiagnosticoId = 1
                    }));

        Assert.Equal(
            "Diagnóstico não encontrado.",
            erro.Message);
    }

    private static Diagnostico
        CriarDiagnosticoConcluido()
    {
        var tecnico = new Tecnico(
            1,
            "Técnico de Teste",
            "Manutenção de equipamentos",
            "Teresópolis",
            "RJ");

        tecnico.AlterarDisponibilidade(true);

        var especialidade = new Especialidade(
            "Manutenção de notebooks",
            "Diagnóstico e reparo de notebooks");

        var selecao = new SelecaoTecnico(
            1,
            tecnico,
            especialidade);

        selecao.Aceitar();

        var diagnostico = new Diagnostico(
            selecao,
            "Equipamento não liga.");

        diagnostico.AlterarInformacoes(
            "Placa de alimentação danificada.",
            "Substituir componente da placa.");

        diagnostico.Concluir();

        return diagnostico;
    }

    private sealed class OrcamentoRepositoryFake
        : IOrcamentoRepository
    {
        public bool ExisteOrcamento { get; set; }

        public Orcamento? OrcamentoAdicionado
            { get; private set; }

        public bool AlteracoesSalvas
            { get; private set; }

        public Task<Orcamento?> ObterPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                OrcamentoAdicionado);
        }

        public Task<Orcamento?>
            ObterPorDiagnosticoAsync(
                int diagnosticoId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                OrcamentoAdicionado);
        }

        public Task<bool> ExisteParaDiagnosticoAsync(
            int diagnosticoId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ExisteOrcamento);
        }

        public Task AdicionarAsync(
            Orcamento orcamento,
            CancellationToken cancellationToken = default)
        {
            OrcamentoAdicionado = orcamento;
            return Task.CompletedTask;
        }

        public Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default)
        {
            AlteracoesSalvas = true;
            return Task.CompletedTask;
        }
    }

    private sealed class DiagnosticoRepositoryFake
        : IDiagnosticoRepository
    {
        private readonly Diagnostico? _diagnostico;

        public DiagnosticoRepositoryFake(
            Diagnostico? diagnostico)
        {
            _diagnostico = diagnostico;
        }

        public Task<Diagnostico?> ObterPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_diagnostico);
        }

        public Task<Diagnostico?>
            ObterPorSolicitacaoAsync(
                int solicitacaoId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_diagnostico);
        }

        public Task<bool> ExisteParaSolicitacaoAsync(
            int solicitacaoId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _diagnostico is not null);
        }

        public Task AdicionarAsync(
            Diagnostico diagnostico,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}