using Assistencia.Api.Enums;
using Assistencia.Api.Models;

namespace Assistencia.Api.Tests;

public class CicloSolicitacaoTests
{
    [Fact]
    public void SolicitacaoNovaDeveIniciarSemHistorico()
    {
        var solicitacao = CriarSolicitacao();

        Assert.Equal(StatusSolicitacao.Aberta, solicitacao.Status);
        Assert.Empty(solicitacao.Historico);
    }

    [Fact]
    public void DeveAvancarParaProximoStatus()
    {
        var solicitacao = CriarSolicitacao();

        solicitacao.AlterarStatus(StatusSolicitacao.AguardandoTecnico);

        Assert.Equal(
            StatusSolicitacao.AguardandoTecnico,
            solicitacao.Status);
    }

    [Fact]
    public void DevePercorrerTodasAsEtapasAteConcluida()
    {
        var solicitacao = CriarSolicitacao();

        AvancarAte(solicitacao, StatusSolicitacao.Concluida);

        Assert.Equal(StatusSolicitacao.Concluida, solicitacao.Status);
    }

    [Theory]
    [InlineData(StatusSolicitacao.Aberta, StatusSolicitacao.EmDiagnostico)]
    [InlineData(StatusSolicitacao.Aberta, StatusSolicitacao.Concluida)]
    [InlineData(StatusSolicitacao.AguardandoTecnico, StatusSolicitacao.OrcamentoEnviado)]
    [InlineData(StatusSolicitacao.OrcamentoAprovado, StatusSolicitacao.Concluida)]
    public void NaoDevePularEtapas(
        StatusSolicitacao atual,
        StatusSolicitacao destino)
    {
        var solicitacao = CriarSolicitacao();
        AvancarAte(solicitacao, atual);
        var registrosAntes = solicitacao.Historico.Count;

        var erro = Assert.Throws<InvalidOperationException>(
            () => solicitacao.AlterarStatus(destino));

        Assert.Equal("Transição de status inválida.", erro.Message);
        Assert.Equal(atual, solicitacao.Status);
        Assert.Equal(registrosAntes, solicitacao.Historico.Count);
    }

    [Theory]
    [InlineData(StatusSolicitacao.AguardandoTecnico, StatusSolicitacao.Aberta)]
    [InlineData(StatusSolicitacao.EmExecucao, StatusSolicitacao.OrcamentoAprovado)]
    [InlineData(StatusSolicitacao.EmExecucao, StatusSolicitacao.Aberta)]
    public void NaoDeveVoltarParaStatusAnterior(
        StatusSolicitacao atual,
        StatusSolicitacao destino)
    {
        var solicitacao = CriarSolicitacao();
        AvancarAte(solicitacao, atual);
        var registrosAntes = solicitacao.Historico.Count;

        Assert.Throws<InvalidOperationException>(
            () => solicitacao.AlterarStatus(destino));

        Assert.Equal(atual, solicitacao.Status);
        Assert.Equal(registrosAntes, solicitacao.Historico.Count);
    }

    [Fact]
    public void NaoDeveAlterarParaOMesmoStatus()
    {
        var solicitacao = CriarSolicitacao();

        Assert.Throws<InvalidOperationException>(
            () => solicitacao.AlterarStatus(StatusSolicitacao.Aberta));

        Assert.Equal(StatusSolicitacao.Aberta, solicitacao.Status);
        Assert.Empty(solicitacao.Historico);
    }

    [Theory]
    [InlineData(StatusSolicitacao.Concluida, StatusSolicitacao.Aberta)]
    [InlineData(StatusSolicitacao.Concluida, StatusSolicitacao.EmExecucao)]
    [InlineData(StatusSolicitacao.Cancelada, StatusSolicitacao.Aberta)]
    [InlineData(StatusSolicitacao.Cancelada, StatusSolicitacao.AguardandoTecnico)]
    public void NaoDeveAlterarSolicitacaoEncerrada(
        StatusSolicitacao encerrada,
        StatusSolicitacao destino)
    {
        var solicitacao = CriarSolicitacao();
        AvancarAte(solicitacao, encerrada);
        var registrosAntes = solicitacao.Historico.Count;

        var erro = Assert.Throws<InvalidOperationException>(
            () => solicitacao.AlterarStatus(destino));

        Assert.Equal(
            "Não é possível alterar uma solicitação encerrada.",
            erro.Message);
        Assert.Equal(encerrada, solicitacao.Status);
        Assert.Equal(registrosAntes, solicitacao.Historico.Count);
    }

    [Theory]
    [InlineData(StatusSolicitacao.Aberta)]
    [InlineData(StatusSolicitacao.AguardandoTecnico)]
    [InlineData(StatusSolicitacao.EmDiagnostico)]
    [InlineData(StatusSolicitacao.OrcamentoEnviado)]
    [InlineData(StatusSolicitacao.OrcamentoAprovado)]
    public void DeveCancelarAntesDaExecucao(StatusSolicitacao atual)
    {
        var solicitacao = CriarSolicitacao();
        AvancarAte(solicitacao, atual);

        solicitacao.Cancelar();

        Assert.Equal(StatusSolicitacao.Cancelada, solicitacao.Status);

        var registro = solicitacao.Historico.Last();
        Assert.Equal(atual, registro.StatusAnterior);
        Assert.Equal(StatusSolicitacao.Cancelada, registro.NovoStatus);
    }

    [Fact]
    public void NaoDeveCancelarEmExecucao()
    {
        AssertCancelamentoRejeitado(StatusSolicitacao.EmExecucao);
    }

    [Fact]
    public void NaoDeveCancelarSolicitacaoConcluida()
    {
        AssertCancelamentoRejeitado(StatusSolicitacao.Concluida);
    }

    [Fact]
    public void NaoDeveCancelarSolicitacaoJaCancelada()
    {
        AssertCancelamentoRejeitado(StatusSolicitacao.Cancelada);
    }

    [Fact]
    public void AlterarStatusParaCanceladaDeveCancelar()
    {
        var solicitacao = CriarSolicitacao();
        AvancarAte(solicitacao, StatusSolicitacao.EmDiagnostico);

        solicitacao.AlterarStatus(StatusSolicitacao.Cancelada);

        Assert.Equal(StatusSolicitacao.Cancelada, solicitacao.Status);
        Assert.Equal(
            StatusSolicitacao.Cancelada,
            solicitacao.Historico.Last().NovoStatus);
    }

    [Fact]
    public void AlterarStatusParaCanceladaEmExecucaoDeveSerRejeitado()
    {
        var solicitacao = CriarSolicitacao();
        AvancarAte(solicitacao, StatusSolicitacao.EmExecucao);
        var registrosAntes = solicitacao.Historico.Count;

        Assert.Throws<InvalidOperationException>(
            () => solicitacao.AlterarStatus(StatusSolicitacao.Cancelada));

        Assert.Equal(StatusSolicitacao.EmExecucao, solicitacao.Status);
        Assert.Equal(registrosAntes, solicitacao.Historico.Count);
    }

    [Fact]
    public void CadaAlteracaoDeveGerarUmRegistroNoHistorico()
    {
        var solicitacao = CriarSolicitacao();

        solicitacao.AlterarStatus(StatusSolicitacao.AguardandoTecnico);
        Assert.Single(solicitacao.Historico);

        solicitacao.AlterarStatus(StatusSolicitacao.EmDiagnostico);
        Assert.Equal(2, solicitacao.Historico.Count);

        solicitacao.Cancelar();
        Assert.Equal(3, solicitacao.Historico.Count);
    }

    [Fact]
    public void HistoricoDeveGuardarStatusAnteriorENovoStatus()
    {
        var solicitacao = CriarSolicitacao();

        AvancarAte(solicitacao, StatusSolicitacao.Concluida);

        var esperado = new[]
        {
            (StatusSolicitacao.Aberta, StatusSolicitacao.AguardandoTecnico),
            (StatusSolicitacao.AguardandoTecnico, StatusSolicitacao.EmDiagnostico),
            (StatusSolicitacao.EmDiagnostico, StatusSolicitacao.OrcamentoEnviado),
            (StatusSolicitacao.OrcamentoEnviado, StatusSolicitacao.OrcamentoAprovado),
            (StatusSolicitacao.OrcamentoAprovado, StatusSolicitacao.EmExecucao),
            (StatusSolicitacao.EmExecucao, StatusSolicitacao.Concluida)
        };

        var registrado = solicitacao.Historico
            .Select(h => (h.StatusAnterior, h.NovoStatus))
            .ToArray();

        Assert.Equal(esperado, registrado);
    }

    [Fact]
    public void RegistroDeveGuardarDataDaAlteracaoEmUtc()
    {
        var solicitacao = CriarSolicitacao();
        var inicio = DateTime.UtcNow;

        solicitacao.AlterarStatus(StatusSolicitacao.AguardandoTecnico);

        var registro = Assert.Single(solicitacao.Historico);
        Assert.Equal(DateTimeKind.Utc, registro.DataAlteracao.Kind);
        Assert.InRange(
            registro.DataAlteracao,
            inicio.AddSeconds(-1),
            DateTime.UtcNow.AddSeconds(1));
        Assert.Same(solicitacao, registro.Solicitacao);
    }

    private static void AssertCancelamentoRejeitado(
        StatusSolicitacao atual)
    {
        var solicitacao = CriarSolicitacao();
        AvancarAte(solicitacao, atual);
        var registrosAntes = solicitacao.Historico.Count;

        var erro = Assert.Throws<InvalidOperationException>(
            () => solicitacao.Cancelar());

        Assert.Equal(
            "Não é possível cancelar esta solicitação.",
            erro.Message);
        Assert.Equal(atual, solicitacao.Status);
        Assert.Equal(registrosAntes, solicitacao.Historico.Count);
    }

    private static Solicitacao CriarSolicitacao()
    {
        var equipamento = new Equipamento { Id = 1, ClienteId = 1 };
        var especialidade = new Especialidade("Notebooks", "");

        return new Solicitacao(
            1,
            equipamento,
            especialidade,
            "Notebook não liga.");
    }

    private static void AvancarAte(
        Solicitacao solicitacao,
        StatusSolicitacao destino)
    {
        if (destino == StatusSolicitacao.Cancelada)
        {
            solicitacao.Cancelar();
            return;
        }

        while (solicitacao.Status < destino)
        {
            solicitacao.AlterarStatus(solicitacao.Status + 1);
        }
    }
}
