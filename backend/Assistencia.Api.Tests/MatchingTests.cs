using Assistencia.Api.Enums;
using Assistencia.Api.Models;

namespace Assistencia.Api.Tests;

public class MatchingTests
{
    [Fact]
    public void DeveCriarSelecaoAguardandoResposta()
    {
        var tecnico = CriarTecnicoDisponivel();
        var especialidade = CriarEspecialidade();

        var selecao = new SelecaoTecnico(
            1,
            tecnico,
            especialidade);

        Assert.Equal(
            StatusSelecaoTecnico.AguardandoResposta,
            selecao.Status);

        Assert.Equal(1, selecao.SolicitacaoId);
        Assert.Null(selecao.DataResposta);
    }

    [Fact]
    public void NaoDeveSelecionarTecnicoIndisponivel()
    {
        var tecnico = new Tecnico(
            1,
            "Técnico Teste",
            "Manutenção de equipamentos",
            "Teresópolis",
            "RJ");

        var especialidade = CriarEspecialidade();

        var erro = Assert.Throws<InvalidOperationException>(
            () => new SelecaoTecnico(
                1,
                tecnico,
                especialidade));

        Assert.Equal(
            "O técnico não está disponível.",
            erro.Message);
    }

    [Fact]
    public void DeveAceitarSelecao()
    {
        var tecnico = CriarTecnicoDisponivel();
        var especialidade = CriarEspecialidade();

        var selecao = new SelecaoTecnico(
            1,
            tecnico,
            especialidade);

        selecao.Aceitar();

        Assert.Equal(
            StatusSelecaoTecnico.Aceita,
            selecao.Status);

        Assert.NotNull(selecao.DataResposta);
    }

    [Fact]
    public void NaoDeveResponderSelecaoDuasVezes()
    {
        var tecnico = CriarTecnicoDisponivel();
        var especialidade = CriarEspecialidade();

        var selecao = new SelecaoTecnico(
            1,
            tecnico,
            especialidade);

        selecao.Recusar();

        Assert.Throws<InvalidOperationException>(
            () => selecao.Aceitar());
    }

    private static Tecnico CriarTecnicoDisponivel()
    {
        var tecnico = new Tecnico(
            1,
            "Técnico Teste",
            "Manutenção de equipamentos",
            "Teresópolis",
            "RJ");

        tecnico.AlterarDisponibilidade(true);

        return tecnico;
    }

    private static Especialidade CriarEspecialidade()
    {
        return new Especialidade(
            "Manutenção de celulares",
            "Reparo de aparelhos celulares");
    }
}