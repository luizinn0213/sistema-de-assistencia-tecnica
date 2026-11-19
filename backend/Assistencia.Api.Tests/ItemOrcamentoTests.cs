using Assistencia.Api.Enums;
using Assistencia.Api.Models;

namespace Assistencia.Api.Tests;

public class ItemOrcamentoTests
{
    [Fact]
    public void DeveCalcularValorTotal()
    {
        var item = new ItemOrcamento(
            "Memória RAM",
            TipoItemOrcamento.Peca,
            2,
            150m);

        Assert.Equal(300m, item.ValorTotal);
    }

    [Fact]
    public void NaoDeveCriarSemDescricao()
    {
        Assert.Throws<ArgumentException>(
            () => new ItemOrcamento(
                " ",
                TipoItemOrcamento.Peca,
                1,
                100m));
    }

    [Fact]
    public void NaoDeveCriarComQuantidadeInvalida()
    {
        var erro = Assert.Throws<ArgumentException>(
            () => new ItemOrcamento(
                "Mão de obra",
                TipoItemOrcamento.Servico,
                0,
                100m));

        Assert.Equal(
            "A quantidade deve ser maior que zero.",
            erro.Message);
    }

    [Fact]
    public void NaoDeveCriarComValorNegativo()
    {
        var erro = Assert.Throws<ArgumentException>(
            () => new ItemOrcamento(
                "Tela",
                TipoItemOrcamento.Peca,
                1,
                -1m));

        Assert.Equal(
            "O valor unitário não pode ser negativo.",
            erro.Message);
    }

    [Fact]
    public void DeveAlterarItemValido()
    {
        var item = new ItemOrcamento(
            "Limpeza",
            TipoItemOrcamento.Servico,
            1,
            80m);

        item.Alterar(
            "Limpeza completa",
            TipoItemOrcamento.Servico,
            2,
            90m);

        Assert.Equal(
            "Limpeza completa",
            item.Descricao);

        Assert.Equal(2, item.Quantidade);
        Assert.Equal(90m, item.ValorUnitario);
        Assert.Equal(180m, item.ValorTotal);
    }
}