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

        Assert.Single(selecao.Historico);
        Assert.Equal(
            StatusSelecaoTecnico.AguardandoResposta,
            selecao.Historico.First().Status);
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
    public void DeveAceitarSelecaoERegistrarHistorico()
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
        Assert.Equal(2, selecao.Historico.Count);

        var ultimoRegistro = selecao.Historico.Last();

        Assert.Equal(
            StatusSelecaoTecnico.Aceita,
            ultimoRegistro.Status);

        Assert.Equal(
            "Solicitação aceita pelo técnico.",
            ultimoRegistro.Observacao);
    }

    [Fact]
    public void DeveRecusarSelecaoComMotivoERegistrarHistorico()
    {
        var tecnico = CriarTecnicoDisponivel();
        var especialidade = CriarEspecialidade();

        var selecao = new SelecaoTecnico(
            1,
            tecnico,
            especialidade);

        selecao.Recusar(
            "Não tenho disponibilidade para esta data.");

        Assert.Equal(
            StatusSelecaoTecnico.Recusada,
            selecao.Status);

        Assert.NotNull(selecao.DataResposta);
        Assert.Equal(2, selecao.Historico.Count);

        var ultimoRegistro = selecao.Historico.Last();

        Assert.Equal(
            StatusSelecaoTecnico.Recusada,
            ultimoRegistro.Status);

        Assert.Contains(
            "Não tenho disponibilidade",
            ultimoRegistro.Observacao);
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

        selecao.Recusar("Atendimento indisponível.");

        var erro = Assert.Throws<InvalidOperationException>(
            () => selecao.Aceitar());

        Assert.Equal(
            "Essa seleção já recebeu uma resposta.",
            erro.Message);
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