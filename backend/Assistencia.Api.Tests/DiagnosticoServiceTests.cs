using Assistencia.Api.Dtos;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Assistencia.Api.Services;
using Assistencia.Api.Services.Contracts;

namespace Assistencia.Api.Tests;

public class DiagnosticoServiceTests
{
    [Fact]
    public async Task DeveCriarDiagnosticoComSelecaoAceita()
    {
        var repositorioDiagnostico =
            new DiagnosticoRepositoryFake();

        var repositorioSelecao =
            new SelecaoTecnicoRepositoryFake(
                CriarSelecaoAceita());

        IDiagnosticoService service =
            new DiagnosticoService(
                repositorioDiagnostico,
                repositorioSelecao);

        var resultado = await service.CriarAsync(
            new CriarDiagnosticoDto
            {
                SolicitacaoId = 1,
                DescricaoDefeito =
                    "Placa de alimentação danificada."
            });

        Assert.NotNull(resultado);
        Assert.Same(
            resultado,
            repositorioDiagnostico.DiagnosticoAdicionado);

        Assert.True(
            repositorioDiagnostico.AlteracoesSalvas);
    }

    [Fact]
    public async Task NaoDeveCriarDiagnosticoDuplicado()
    {
        var repositorioDiagnostico =
            new DiagnosticoRepositoryFake
            {
                ExisteDiagnostico = true
            };

        var repositorioSelecao =
            new SelecaoTecnicoRepositoryFake(
                CriarSelecaoAceita());

        var service = new DiagnosticoService(
            repositorioDiagnostico,
            repositorioSelecao);

        var erro =
            await Assert.ThrowsAsync<
                InvalidOperationException>(
                () => service.CriarAsync(
                    new CriarDiagnosticoDto
                    {
                        SolicitacaoId = 1,
                        DescricaoDefeito =
                            "Equipamento não liga."
                    }));

        Assert.Equal(
            "Essa solicitação já possui um diagnóstico.",
            erro.Message);

        Assert.Null(
            repositorioDiagnostico.DiagnosticoAdicionado);
    }

    [Fact]
    public async Task NaoDeveCriarSemSelecaoAceita()
    {
        var repositorioDiagnostico =
            new DiagnosticoRepositoryFake();

        var repositorioSelecao =
            new SelecaoTecnicoRepositoryFake(null);

        var service = new DiagnosticoService(
            repositorioDiagnostico,
            repositorioSelecao);

        var erro =
            await Assert.ThrowsAsync<
                InvalidOperationException>(
                () => service.CriarAsync(
                    new CriarDiagnosticoDto
                    {
                        SolicitacaoId = 1,
                        DescricaoDefeito =
                            "Tela danificada."
                    }));

        Assert.Equal(
            "A solicitação não possui uma seleção aceita.",
            erro.Message);

        Assert.Null(
            repositorioDiagnostico.DiagnosticoAdicionado);
    }

    private static SelecaoTecnico CriarSelecaoAceita()
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

        return selecao;
    }

    private sealed class DiagnosticoRepositoryFake
        : IDiagnosticoRepository
    {
        public bool ExisteDiagnostico { get; set; }

        public Diagnostico? DiagnosticoAdicionado
            { get; private set; }

        public bool AlteracoesSalvas
            { get; private set; }

        public Task<Diagnostico?> ObterPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                DiagnosticoAdicionado);
        }

        public Task<Diagnostico?>
            ObterPorSolicitacaoAsync(
                int solicitacaoId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                DiagnosticoAdicionado);
        }

        public Task<bool> ExisteParaSolicitacaoAsync(
            int solicitacaoId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ExisteDiagnostico);
        }

        public Task AdicionarAsync(
            Diagnostico diagnostico,
            CancellationToken cancellationToken = default)
        {
            DiagnosticoAdicionado = diagnostico;
            return Task.CompletedTask;
        }

        public Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default)
        {
            AlteracoesSalvas = true;
            return Task.CompletedTask;
        }
    }

    private sealed class SelecaoTecnicoRepositoryFake
        : ISelecaoTecnicoRepository
    {
        private readonly SelecaoTecnico? _selecao;

        public SelecaoTecnicoRepositoryFake(
            SelecaoTecnico? selecao)
        {
            _selecao = selecao;
        }

        public Task<SelecaoTecnico?>
            ObterAceitaPorSolicitacaoAsync(
                int solicitacaoId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_selecao);
        }
    }
}
