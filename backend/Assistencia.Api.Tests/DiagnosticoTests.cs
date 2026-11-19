using Assistencia.Api.Enums;
using Assistencia.Api.Models;

namespace Assistencia.Api.Tests;

public class DiagnosticoTests
{
    [Fact]
    public void DeveCriarDiagnosticoParaSelecaoAceita()
    {
        var selecao = CriarSelecaoAceita();

        var diagnostico = new Diagnostico(
            selecao,
            "Conector de energia danificado.");

        Assert.Equal(
            StatusDiagnostico.EmElaboracao,
            diagnostico.Status);

        Assert.Equal(
            selecao.SolicitacaoId,
            diagnostico.SolicitacaoId);

        Assert.Equal(
            selecao.TecnicoId,
            diagnostico.TecnicoId);

        Assert.Null(diagnostico.DataConclusao);
    }

    [Fact]
    public void NaoDeveCriarDiagnosticoAntesDoAceite()
    {
        var selecao = CriarSelecaoAguardandoResposta();

        var erro = Assert.Throws<InvalidOperationException>(
            () => new Diagnostico(
                selecao,
                "Conector danificado."));

        Assert.Equal(
            "O diagnóstico somente pode ser criado após " +
            "o técnico aceitar a solicitação.",
            erro.Message);
    }

    [Fact]
    public void NaoDeveCriarSemDescricaoDoDefeito()
    {
        var selecao = CriarSelecaoAceita();

        Assert.Throws<ArgumentException>(
            () => new Diagnostico(selecao, " "));
    }

    [Fact]
    public void DeveAlterarInformacoesEmElaboracao()
    {
        var diagnostico = CriarDiagnostico();

        diagnostico.AlterarInformacoes(
            "Placa com componente queimado.",
            "Substituir o componente danificado.");

        Assert.Equal(
            "Placa com componente queimado.",
            diagnostico.DescricaoDefeito);

        Assert.Equal(
            "Substituir o componente danificado.",
            diagnostico.SolucaoRecomendada);
    }

    [Fact]
    public void DeveConcluirDiagnosticoValido()
    {
        var diagnostico = CriarDiagnostico();

        diagnostico.AlterarInformacoes(
            "Tela danificada.",
            "Substituir o conjunto da tela.");

        diagnostico.Concluir();

        Assert.Equal(
            StatusDiagnostico.Concluido,
            diagnostico.Status);

        Assert.NotNull(diagnostico.DataConclusao);
    }

    [Fact]
    public void NaoDeveConcluirSemSolucao()
    {
        var diagnostico = CriarDiagnostico();

        Assert.Throws<InvalidOperationException>(
            () => diagnostico.Concluir());
    }

    [Fact]
    public void NaoDeveAlterarDiagnosticoConcluido()
    {
        var diagnostico = CriarDiagnostico();

        diagnostico.AlterarInformacoes(
            "Tela danificada.",
            "Substituir a tela.");

        diagnostico.Concluir();

        Assert.Throws<InvalidOperationException>(
            () => diagnostico.AlterarInformacoes(
                "Nova descrição.",
                "Nova solução."));
    }

    private static Diagnostico CriarDiagnostico()
    {
        return new Diagnostico(
            CriarSelecaoAceita(),
            "Equipamento não liga.");
    }

    private static SelecaoTecnico CriarSelecaoAceita()
    {
        var selecao =
            CriarSelecaoAguardandoResposta();

        selecao.Aceitar();

        return selecao;
    }

    private static SelecaoTecnico
        CriarSelecaoAguardandoResposta()
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

        return new SelecaoTecnico(
            1,
            tecnico,
            especialidade);
    }
}