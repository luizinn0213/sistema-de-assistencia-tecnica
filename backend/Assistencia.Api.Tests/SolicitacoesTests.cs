using System.Text.Json;
using Assistencia.Api.Controllers;
using Assistencia.Api.Data;
using Assistencia.Api.Dtos;
using Assistencia.Api.Enums;
using Assistencia.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Tests;

public class SolicitacoesTests
{
    [Fact]
    public async Task DeveAbrirSolicitacaoValida()
    {
        using var banco = CriarBanco();

        var cliente = await CriarCliente(banco.Context, "a");
        var equipamento = await CriarEquipamento(
            banco.Context, cliente, "SERIE-A");
        var especialidade = await CriarEspecialidade(
            banco.Context);

        var controller = new SolicitacoesController(
            banco.Context);

        var inicio = DateTime.UtcNow;

        var resultado = await controller.Abrir(
            cliente.Id,
            new CriarSolicitacaoDto
            {
                EquipamentoId = equipamento.Id,
                EspecialidadeId = especialidade.Id,
                DescricaoProblema = "  Notebook não liga.  "
            });

        Assert.IsType<CreatedAtActionResult>(resultado);

        var solicitacao =
            await banco.Context.Solicitacoes.SingleAsync();

        Assert.Equal(cliente.Id, solicitacao.ClienteId);
        Assert.Equal(equipamento.Id, solicitacao.EquipamentoId);
        Assert.Equal(especialidade.Id, solicitacao.EspecialidadeId);
        Assert.Equal("Notebook não liga.", solicitacao.DescricaoProblema);
        Assert.StartsWith("SOL-", solicitacao.Numero);
        Assert.InRange(
            solicitacao.DataCriacao,
            inicio.AddSeconds(-1),
            DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void SolicitacaoDeveIniciarComStatusAberta()
    {
        var equipamento = new Equipamento { Id = 1, ClienteId = 1 };
        var especialidade = new Especialidade("Notebooks", "");

        var solicitacao = new Solicitacao(
            1,
            equipamento,
            especialidade,
            "Tela quebrada.");

        Assert.Equal(StatusSolicitacao.Aberta, solicitacao.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public async Task NaoDeveAbrirComDescricaoVazia(
        string descricao)
    {
        using var banco = CriarBanco();

        var cenario = await CriarCenario(banco.Context);

        var resultado = await cenario.Controller.Abrir(
            cenario.Cliente.Id,
            new CriarSolicitacaoDto
            {
                EquipamentoId = cenario.Equipamento.Id,
                EspecialidadeId = cenario.Especialidade.Id,
                DescricaoProblema = descricao
            });

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Empty(banco.Context.Solicitacoes);
    }

    [Fact]
    public async Task NaoDeveAbrirComDescricaoAcimaDoLimite()
    {
        using var banco = CriarBanco();

        var cenario = await CriarCenario(banco.Context);

        var resultado = await cenario.Controller.Abrir(
            cenario.Cliente.Id,
            new CriarSolicitacaoDto
            {
                EquipamentoId = cenario.Equipamento.Id,
                EspecialidadeId = cenario.Especialidade.Id,
                DescricaoProblema = new string(
                    'a',
                    Solicitacao.TamanhoMaximoDescricao + 1)
            });

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Empty(banco.Context.Solicitacoes);
    }

    [Fact]
    public async Task NaoDeveAbrirComEspecialidadeInexistente()
    {
        using var banco = CriarBanco();

        var cenario = await CriarCenario(banco.Context);

        var resultado = await cenario.Controller.Abrir(
            cenario.Cliente.Id,
            new CriarSolicitacaoDto
            {
                EquipamentoId = cenario.Equipamento.Id,
                EspecialidadeId = 999,
                DescricaoProblema = "Não liga."
            });

        Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.Empty(banco.Context.Solicitacoes);
    }

    [Fact]
    public async Task NaoDeveAbrirComEspecialidadeInativa()
    {
        using var banco = CriarBanco();

        var cenario = await CriarCenario(banco.Context);

        cenario.Especialidade.Inativar();
        await banco.Context.SaveChangesAsync();

        var resultado = await cenario.Controller.Abrir(
            cenario.Cliente.Id,
            new CriarSolicitacaoDto
            {
                EquipamentoId = cenario.Equipamento.Id,
                EspecialidadeId = cenario.Especialidade.Id,
                DescricaoProblema = "Não liga."
            });

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Empty(banco.Context.Solicitacoes);
    }

    [Fact]
    public async Task NaoDeveAbrirComEquipamentoInexistente()
    {
        using var banco = CriarBanco();

        var cenario = await CriarCenario(banco.Context);

        var resultado = await cenario.Controller.Abrir(
            cenario.Cliente.Id,
            new CriarSolicitacaoDto
            {
                EquipamentoId = 999,
                EspecialidadeId = cenario.Especialidade.Id,
                DescricaoProblema = "Não liga."
            });

        Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.Empty(banco.Context.Solicitacoes);
    }

    [Fact]
    public async Task NaoDeveAbrirComEquipamentoDeOutroCliente()
    {
        using var banco = CriarBanco();

        var cenario = await CriarCenario(banco.Context);

        var outroCliente = await CriarCliente(
            banco.Context, "b");
        var equipamentoDeOutro = await CriarEquipamento(
            banco.Context, outroCliente, "SERIE-B");

        var resultado = await cenario.Controller.Abrir(
            cenario.Cliente.Id,
            new CriarSolicitacaoDto
            {
                EquipamentoId = equipamentoDeOutro.Id,
                EspecialidadeId = cenario.Especialidade.Id,
                DescricaoProblema = "Não liga."
            });

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Empty(banco.Context.Solicitacoes);
    }

    [Fact]
    public async Task NaoDeveAbrirParaClienteInexistente()
    {
        using var banco = CriarBanco();

        var cenario = await CriarCenario(banco.Context);

        var resultado = await cenario.Controller.Abrir(
            999,
            new CriarSolicitacaoDto
            {
                EquipamentoId = cenario.Equipamento.Id,
                EspecialidadeId = cenario.Especialidade.Id,
                DescricaoProblema = "Não liga."
            });

        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task SolicitacoesDevemReceberNumerosDiferentes()
    {
        using var banco = CriarBanco();

        var cenario = await CriarCenario(banco.Context);

        for (var i = 0; i < 2; i++)
        {
            await cenario.Controller.Abrir(
                cenario.Cliente.Id,
                new CriarSolicitacaoDto
                {
                    EquipamentoId = cenario.Equipamento.Id,
                    EspecialidadeId = cenario.Especialidade.Id,
                    DescricaoProblema = $"Problema {i}"
                });
        }

        var numeros = await banco.Context.Solicitacoes
            .Select(s => s.Numero)
            .ToListAsync();

        Assert.Equal(2, numeros.Count);
        Assert.NotEqual(numeros[0], numeros[1]);
    }

    [Fact]
    public async Task BancoDeveRejeitarNumeroRepetido()
    {
        using var banco = CriarBanco();

        var cenario = await CriarCenario(banco.Context);

        var primeira = new Solicitacao(
            cenario.Cliente.Id,
            cenario.Equipamento,
            cenario.Especialidade,
            "Primeira");

        var segunda = new Solicitacao(
            cenario.Cliente.Id,
            cenario.Equipamento,
            cenario.Especialidade,
            "Segunda");

        banco.Context.Solicitacoes.Add(primeira);
        await banco.Context.SaveChangesAsync();

        banco.Context.Solicitacoes.Add(segunda);
        banco.Context.Entry(segunda)
            .Property(s => s.Numero)
            .CurrentValue = primeira.Numero;

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            banco.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task ClienteDeveListarSomenteSuasSolicitacoes()
    {
        using var banco = CriarBanco();

        var cenario = await CriarCenario(banco.Context);

        var outroCliente = await CriarCliente(
            banco.Context, "b");
        var equipamentoDeOutro = await CriarEquipamento(
            banco.Context, outroCliente, "SERIE-B");

        await cenario.Controller.Abrir(
            cenario.Cliente.Id,
            new CriarSolicitacaoDto
            {
                EquipamentoId = cenario.Equipamento.Id,
                EspecialidadeId = cenario.Especialidade.Id,
                DescricaoProblema = "Do cliente A"
            });

        await cenario.Controller.Abrir(
            outroCliente.Id,
            new CriarSolicitacaoDto
            {
                EquipamentoId = equipamentoDeOutro.Id,
                EspecialidadeId = cenario.Especialidade.Id,
                DescricaoProblema = "Do cliente B"
            });

        var resultado = await cenario.Controller.Listar(
            cenario.Cliente.Id);

        var lista = LerJson(
            Assert.IsType<OkObjectResult>(resultado));

        var item = Assert.Single(lista.EnumerateArray());

        Assert.Equal(
            "Do cliente A",
            item.GetProperty("DescricaoProblema").GetString());
    }

    [Fact]
    public async Task ClienteNaoDeveConsultarSolicitacaoDeOutroCliente()
    {
        using var banco = CriarBanco();

        var cenario = await CriarCenario(banco.Context);

        var outroCliente = await CriarCliente(
            banco.Context, "b");

        await cenario.Controller.Abrir(
            cenario.Cliente.Id,
            new CriarSolicitacaoDto
            {
                EquipamentoId = cenario.Equipamento.Id,
                EspecialidadeId = cenario.Especialidade.Id,
                DescricaoProblema = "Do cliente A"
            });

        var solicitacao =
            await banco.Context.Solicitacoes.SingleAsync();

        var resultadoDono = await cenario.Controller.Obter(
            cenario.Cliente.Id,
            solicitacao.Id);

        var resultadoOutro = await cenario.Controller.Obter(
            outroCliente.Id,
            solicitacao.Id);

        Assert.IsType<OkObjectResult>(resultadoDono);
        Assert.IsType<NotFoundObjectResult>(resultadoOutro);
    }

    private static JsonElement LerJson(OkObjectResult resultado)
    {
        return JsonSerializer.SerializeToElement(resultado.Value);
    }

    private static async Task<Cenario> CriarCenario(
        AppDbContext context)
    {
        var cliente = await CriarCliente(context, "a");
        var equipamento = await CriarEquipamento(
            context, cliente, "SERIE-A");
        var especialidade = await CriarEspecialidade(context);

        return new Cenario(
            new SolicitacoesController(context),
            cliente,
            equipamento,
            especialidade);
    }

    private sealed record Cenario(
        SolicitacoesController Controller,
        Cliente Cliente,
        Equipamento Equipamento,
        Especialidade Especialidade);

    private static async Task<Cliente> CriarCliente(
        AppDbContext context,
        string identificador)
    {
        var cliente = new Cliente
        {
            Usuario = new Usuario
            {
                Nome = $"Cliente {identificador}",
                Email = $"{identificador}@email.com",
                SenhaHash = "hash",
                Tipo = TipoUsuario.Cliente
            },
            CpfCnpj = identificador == "a"
                ? "11111111111"
                : "22222222222"
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        return cliente;
    }

    private static async Task<Equipamento> CriarEquipamento(
        AppDbContext context,
        Cliente cliente,
        string numeroSerie)
    {
        var equipamento = new Equipamento
        {
            ClienteId = cliente.Id,
            Tipo = "Notebook",
            Marca = "Dell",
            Modelo = "Inspiron",
            NumeroSerie = numeroSerie
        };

        context.Equipamentos.Add(equipamento);
        await context.SaveChangesAsync();

        return equipamento;
    }

    private static async Task<Especialidade> CriarEspecialidade(
        AppDbContext context)
    {
        var especialidade = new Especialidade(
            "Notebooks",
            "Manutenção de notebooks");

        context.Especialidades.Add(especialidade);
        await context.SaveChangesAsync();

        return especialidade;
    }

    private static BancoTeste CriarBanco()
    {
        return new BancoTeste();
    }

    private sealed class BancoTeste : IDisposable
    {
        private readonly SqliteConnection _conexao;

        public AppDbContext Context { get; }

        public BancoTeste()
        {
            _conexao = new SqliteConnection(
                "Data Source=:memory:");

            _conexao.Open();

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(_conexao)
                    .Options;

            Context = new AppDbContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Dispose();
            _conexao.Dispose();
        }
    }
}
