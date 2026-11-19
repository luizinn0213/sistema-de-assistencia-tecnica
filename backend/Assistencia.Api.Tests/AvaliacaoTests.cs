using Assistencia.Api.Models;

namespace Assistencia.Api.Tests;

public class AvaliacaoTests
{
    [Fact]
    public void DeveCriarAvaliacaoParaExecucaoConcluida()
    {
        var execucao = CriarExecucaoConcluida();

        var avaliacao = new Avaliacao(
            execucao,
            1,
            5,
            "Serviço bem executado.");

        Assert.Equal(1, avaliacao.ClienteId);
        Assert.Equal(5, avaliacao.Nota);
        Assert.Equal("Serviço bem executado.", avaliacao.Comentario);
        Assert.NotEqual(default, avaliacao.DataAvaliacao);
    }

    [Fact]
    public void NaoDeveAvaliarExecucaoNaoConcluida()
    {
        var execucao = new ExecucaoServico(
            ExecucaoServicoTests.CriarOrcamentoAprovado());

        Assert.Throws<InvalidOperationException>(
            () => new Avaliacao(execucao, 1, 5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void NaoDeveAceitarNotaForaDoIntervalo(int nota)
    {
        var execucao = CriarExecucaoConcluida();

        Assert.Throws<ArgumentException>(
            () => new Avaliacao(execucao, 1, nota));
    }

    [Fact]
    public void NaoDeveCriarSemCliente()
    {
        var execucao = CriarExecucaoConcluida();

        Assert.Throws<ArgumentException>(
            () => new Avaliacao(execucao, 0, 5));
    }

    [Fact]
    public void DevePermitirAvaliacaoSemComentario()
    {
        var avaliacao = new Avaliacao(
            CriarExecucaoConcluida(),
            1,
            4);

        Assert.Null(avaliacao.Comentario);
    }

    [Fact]
    public void NaoDeveAceitarComentarioMuitoLongo()
    {
        var comentario = new string(
            'a',
            Avaliacao.TamanhoMaximoComentario + 1);

        Assert.Throws<ArgumentException>(
            () => new Avaliacao(
                CriarExecucaoConcluida(),
                1,
                5,
                comentario));
    }

    private static ExecucaoServico CriarExecucaoConcluida()
    {
        var execucao = new ExecucaoServico(
            ExecucaoServicoTests.CriarOrcamentoAprovado());
        execucao.Iniciar();
        execucao.Concluir();
        return execucao;
    }
}
