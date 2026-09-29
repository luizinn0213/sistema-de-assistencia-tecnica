using Assistencia.Api.Controllers;
using Assistencia.Api.Data;
using Assistencia.Api.Dtos;
using Assistencia.Api.Enums;
using Assistencia.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Assistencia.Api.Tests;

public class OrcamentoTests
{
    private static AppDbContext CriarContexto()
    {
        var opcoes =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=:memory:")
                .Options;

        var contexto = new AppDbContext(opcoes);

        contexto.Database.OpenConnection();
        contexto.Database.EnsureCreated();

        return contexto;
    }

    private static CriarItemDto CriarItem(
        decimal valor,
        int quantidade = 1)
    {
        return new CriarItemDto
        {
            Descricao = "Serviço de teste",
            Tipo = TipoItemOrcamento.Servico,
            Quantidade = quantidade,
            ValorUnitario = valor
        };
    }

    [Fact]
    public async Task CriarSemDiagnostico_DeveFalhar()
    {
        using var contexto = CriarContexto();

        var controller =
            new OrcamentosController(contexto);

        var dados = new CriarOrcamentoDto
        {
            DiagnosticoId = 1,
            DiagnosticoConcluido = false,
            Itens = new List<CriarItemDto>
            {
                CriarItem(100)
            }
        };

        var resultado = await controller.Criar(dados);

        Assert.IsType<BadRequestObjectResult>(
            resultado);
    }

    [Fact]
    public async Task Criar_DeveCalcularTotal()
    {
        using var contexto = CriarContexto();

        var controller =
            new OrcamentosController(contexto);

        var dados = new CriarOrcamentoDto
        {
            DiagnosticoId = 1,
            DiagnosticoConcluido = true,
            Itens = new List<CriarItemDto>
            {
                CriarItem(100, 2),
                CriarItem(50)
            }
        };

        var resultado = await controller.Criar(dados);

        var criado =
            Assert.IsType<CreatedAtActionResult>(
                resultado);

        var orcamento =
            Assert.IsType<Orcamento>(criado.Value);

        Assert.Equal(250m, orcamento.ValorTotal);

        Assert.Equal(
            StatusOrcamento.Pendente,
            orcamento.Status);

        Assert.Single(orcamento.Historico);
        Assert.Equal(2, orcamento.Itens.Count);
    }

    [Fact]
    public async Task NovaVersao_DeveVoltarParaPendente()
    {
        using var contexto = CriarContexto();

        var orcamento = new Orcamento
        {
            DiagnosticoId = 1,
            DiagnosticoConcluido = true,
            Versao = 1,
            Status = StatusOrcamento.Aprovado,
            ValorTotal = 100,
            DataResposta = DateTime.UtcNow,
            Comissao = new Comissao
            {
                Porcentagem = 10,
                ValorComissao = 10
            }
        };

        orcamento.Itens.Add(
            new ItemOrcamento
            {
                Descricao = "Item antigo",
                Tipo = TipoItemOrcamento.Servico,
                Quantidade = 1,
                ValorUnitario = 100
            });

        orcamento.Historico.Add(
            new HistoricoOrcamento
            {
                Versao = 1,
                ValorTotal = 100,
                Status = StatusOrcamento.Aprovado,
                Obs = "Versão inicial."
            });

        contexto.Orcamentos.Add(orcamento);
        await contexto.SaveChangesAsync();

        var controller =
            new OrcamentosController(contexto);

        var novosItens = new List<CriarItemDto>
        {
            CriarItem(300)
        };

        var resultado =
            await controller.CriarNovaVersao(
                orcamento.Id,
                novosItens);

        var resposta =
            Assert.IsType<OkObjectResult>(
                resultado);

        var atualizado =
            Assert.IsType<Orcamento>(
                resposta.Value);

        Assert.Equal(2, atualizado.Versao);
        Assert.Equal(300m, atualizado.ValorTotal);

        Assert.Equal(
            StatusOrcamento.Pendente,
            atualizado.Status);

        Assert.Null(atualizado.DataResposta);
        Assert.Null(atualizado.Comissao);
        Assert.Equal(2, atualizado.Historico.Count);
    }

    [Fact]
    public async Task PagamentoPendente_DeveFalhar()
    {
        using var contexto = CriarContexto();

        var orcamento = new Orcamento
        {
            DiagnosticoId = 1,
            DiagnosticoConcluido = true,
            Status = StatusOrcamento.Pendente,
            ValorTotal = 200
        };

        contexto.Orcamentos.Add(orcamento);
        await contexto.SaveChangesAsync();

        var controller =
            new OrcamentosController(contexto);

        var dados = new SimularPagamentoDto
        {
            FormaPagamento = "Pix"
        };

        var resultado =
            await controller.SimularPagamento(
                orcamento.Id,
                dados);

        Assert.IsType<BadRequestObjectResult>(
            resultado);
    }

    [Fact]
    public async Task PagamentoDuplicado_DeveFalhar()
    {
        using var contexto = CriarContexto();

        var orcamento = new Orcamento
        {
            DiagnosticoId = 1,
            DiagnosticoConcluido = true,
            Status = StatusOrcamento.Aprovado,
            ValorTotal = 500
        };

        contexto.Orcamentos.Add(orcamento);
        await contexto.SaveChangesAsync();

        var controller =
            new OrcamentosController(contexto);

        var dados = new SimularPagamentoDto
        {
            FormaPagamento = "Pix"
        };

        var primeiroResultado =
            await controller.SimularPagamento(
                orcamento.Id,
                dados);

        var segundoResultado =
            await controller.SimularPagamento(
                orcamento.Id,
                dados);

        Assert.IsType<OkObjectResult>(
            primeiroResultado);

        Assert.IsType<ConflictObjectResult>(
            segundoResultado);
    }
}