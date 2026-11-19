using Assistencia.Api.Enums;
using Assistencia.Api.Models;

namespace Assistencia.Api.Tests;

public class ExecucaoServicoTests
{
    [Fact]
    public void DeveCriarExecucaoAguardandoInicio()
    {
        var execucao = new ExecucaoServico(CriarOrcamentoAprovado());

        Assert.Equal(StatusExecucaoServico.AguardandoInicio, execucao.Status);
        Assert.Null(execucao.DataInicio);
        Assert.Null(execucao.DataConclusao);
        Assert.Single(execucao.Historico);
    }

    [Fact]
    public void NaoDeveCriarParaOrcamentoNaoAprovado()
    {
        var orcamento = CriarOrcamentoEnviado();

        var erro = Assert.Throws<InvalidOperationException>(
            () => new ExecucaoServico(orcamento));

        Assert.Equal(
            "A execução somente pode ser criada para um orçamento aprovado.",
            erro.Message);
    }

    [Fact]
    public void DeveIniciarExecucao()
    {
        var execucao = new ExecucaoServico(CriarOrcamentoAprovado());

        execucao.Iniciar("Peças separadas para o reparo.");

        Assert.Equal(StatusExecucaoServico.EmAndamento, execucao.Status);
        Assert.NotNull(execucao.DataInicio);
        Assert.Equal("Peças separadas para o reparo.", execucao.ObservacaoInicio);
        Assert.Equal(2, execucao.Historico.Count);
    }

    [Fact]
    public void NaoDeveIniciarDuasVezes()
    {
        var execucao = new ExecucaoServico(CriarOrcamentoAprovado());
        execucao.Iniciar();

        Assert.Throws<InvalidOperationException>(() => execucao.Iniciar());
    }

    [Fact]
    public void DeveConcluirExecucaoEmAndamento()
    {
        var execucao = new ExecucaoServico(CriarOrcamentoAprovado());
        execucao.Iniciar();

        execucao.Concluir("Equipamento testado e funcionando.");

        Assert.Equal(StatusExecucaoServico.Concluida, execucao.Status);
        Assert.NotNull(execucao.DataConclusao);
        Assert.Equal("Equipamento testado e funcionando.", execucao.ObservacaoFinal);
        Assert.Equal(3, execucao.Historico.Count);
    }

    [Fact]
    public void NaoDeveConcluirAntesDoInicio()
    {
        var execucao = new ExecucaoServico(CriarOrcamentoAprovado());

        Assert.Throws<InvalidOperationException>(() => execucao.Concluir());
    }

    [Fact]
    public void NaoDeveAceitarObservacaoMuitoLonga()
    {
        var execucao = new ExecucaoServico(CriarOrcamentoAprovado());
        var texto = new string('a', ExecucaoServico.TamanhoMaximoObservacao + 1);

        Assert.Throws<ArgumentException>(() => execucao.Iniciar(texto));
    }

    internal static Orcamento CriarOrcamentoAprovado()
    {
        var orcamento = CriarOrcamentoEnviado();
        orcamento.Aprovar();
        return orcamento;
    }

    private static Orcamento CriarOrcamentoEnviado()
    {
        var tecnico = new Tecnico(
            1,
            "Técnico Teste",
            "Manutenção de equipamentos",
            "Teresópolis",
            "RJ");
        tecnico.AlterarDisponibilidade(true);

        var especialidade = new Especialidade(
            "Manutenção de notebooks",
            "Reparo de notebooks");

        var selecao = new SelecaoTecnico(1, tecnico, especialidade);
        selecao.Aceitar();

        var diagnostico = new Diagnostico(
            selecao,
            "Placa de alimentação danificada.");
        diagnostico.AlterarInformacoes(
            "Placa de alimentação danificada.",
            "Substituir o componente danificado.");
        diagnostico.Concluir();

        var orcamento = new Orcamento(diagnostico);
        orcamento.AdicionarItem(
            "Componente eletrônico",
            TipoItemOrcamento.Peca,
            1,
            150m);
        orcamento.Enviar();
        return orcamento;
    }
}
