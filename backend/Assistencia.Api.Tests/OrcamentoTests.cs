using Assistencia.Api.Enums;
using Assistencia.Api.Models;

namespace Assistencia.Api.Tests;

public class OrcamentoTests
{
    [Fact]
    public void DeveCriarParaDiagnosticoConcluido()
    {
        var orcamento = CriarOrcamento();

        Assert.Equal(
            StatusOrcamento.EmElaboracao,
            orcamento.Status);

        Assert.Empty(orcamento.Itens);
        Assert.Single(orcamento.Historico);
        Assert.Equal(0m, orcamento.ValorTotal);
    }

    [Fact]
    public void NaoDeveCriarParaDiagnosticoEmElaboracao()
    {
        var diagnostico =
            CriarDiagnosticoEmElaboracao();

        var erro = Assert.Throws<
            InvalidOperationException>(
                () => new Orcamento(diagnostico));

        Assert.Equal(
            "O orçamento somente pode ser criado para " +
            "um diagnóstico concluído.",
            erro.Message);
    }

    [Fact]
    public void DeveAdicionarItensECalcularTotal()
    {
        var orcamento = CriarOrcamento();

        orcamento.AdicionarItem(
            "Tela",
            TipoItemOrcamento.Peca,
            1,
            350m);

        orcamento.AdicionarItem(
            "Mão de obra",
            TipoItemOrcamento.Servico,
            2,
            75m);

        Assert.Equal(2, orcamento.Itens.Count);
        Assert.Equal(500m, orcamento.ValorTotal);
    }

    [Fact]
    public void NaoDeveEnviarOrcamentoSemItens()
    {
        var orcamento = CriarOrcamento();

        var erro = Assert.Throws<
            InvalidOperationException>(
                () => orcamento.Enviar());

        Assert.Equal(
            "O orçamento precisa possuir pelo menos " +
            "um item antes do envio.",
            erro.Message);
    }

    [Fact]
    public void DeveEnviarOrcamentoComItens()
    {
        var orcamento =
            CriarOrcamentoComItem();

        orcamento.Enviar();

        Assert.Equal(
            StatusOrcamento.Enviado,
            orcamento.Status);

        Assert.NotNull(orcamento.DataEnvio);
        Assert.Equal(2, orcamento.Historico.Count);
    }

    [Fact]
    public void DeveAprovarOrcamentoEnviado()
    {
        var orcamento =
            CriarOrcamentoEnviado();

        orcamento.Aprovar(
            "Cliente confirmou o serviço.");

        Assert.Equal(
            StatusOrcamento.Aprovado,
            orcamento.Status);

        Assert.NotNull(orcamento.DataResposta);
        Assert.Equal(3, orcamento.Historico.Count);

        Assert.Contains(
            "Cliente confirmou o serviço",
            orcamento.Historico.Last().Observacao);
    }

    [Fact]
    public void DeveRejeitarOrcamentoEnviado()
    {
        var orcamento =
            CriarOrcamentoEnviado();

        orcamento.Rejeitar(
            "Valor acima do esperado.");

        Assert.Equal(
            StatusOrcamento.Rejeitado,
            orcamento.Status);

        Assert.NotNull(orcamento.DataResposta);

        Assert.Contains(
            "Valor acima do esperado",
            orcamento.Historico.Last().Observacao);
    }

    [Fact]
    public void NaoDeveResponderOrcamentoDuasVezes()
    {
        var orcamento =
            CriarOrcamentoEnviado();

        orcamento.Aprovar();

        Assert.Throws<InvalidOperationException>(
            () => orcamento.Rejeitar(
                "Mudança de decisão."));
    }

    [Fact]
    public void NaoDeveAlterarItensDepoisDoEnvio()
    {
        var orcamento =
            CriarOrcamentoEnviado();

        Assert.Throws<InvalidOperationException>(
            () => orcamento.AdicionarItem(
                "Novo serviço",
                TipoItemOrcamento.Servico,
                1,
                50m));
    }

    [Fact]
    public void DeveRemoverItemDuranteElaboracao()
    {
        var orcamento = CriarOrcamento();

        var item = orcamento.AdicionarItem(
            "Peça temporária",
            TipoItemOrcamento.Peca,
            1,
            100m);

        orcamento.RemoverItem(item);

        Assert.Empty(orcamento.Itens);
        Assert.Equal(0m, orcamento.ValorTotal);
    }

    private static Orcamento CriarOrcamento()
    {
        var diagnostico =
            CriarDiagnosticoEmElaboracao();

        diagnostico.AlterarInformacoes(
            "Placa principal danificada.",
            "Substituir componente da placa.");

        diagnostico.Concluir();

        return new Orcamento(diagnostico);
    }

    private static Orcamento
        CriarOrcamentoComItem()
    {
        var orcamento = CriarOrcamento();

        orcamento.AdicionarItem(
            "Mão de obra",
            TipoItemOrcamento.Servico,
            1,
            120m);

        return orcamento;
    }

    private static Orcamento CriarOrcamentoEnviado()
    {
        var orcamento =
            CriarOrcamentoComItem();

        orcamento.Enviar();

        return orcamento;
    }

    private static Diagnostico
        CriarDiagnosticoEmElaboracao()
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

        return new Diagnostico(
            selecao,
            "Equipamento não liga.");
    }
}