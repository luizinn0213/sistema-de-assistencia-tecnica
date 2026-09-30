using Assistencia.Api.Controllers;
using Assistencia.Api.Data;
using Assistencia.Api.Dtos;
using Assistencia.Api.Enums;
using Assistencia.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Assistencia.Api.Tests;

public class ClientesEquipamentosTests
{
    [Fact]
    public async Task DeveCadastrarClienteComEndereco()
    {
        using var banco = CriarBanco();

        var controller = new ClientesController(
            banco.Context);

        var dados = new CriarClienteDto
        {
            Nome = "João da Silva",
            Email = "joao@email.com",
            Senha = "123456",
            CpfCnpj = "12345678901",
            Endereco = new EnderecoDto
            {
                Cep = "25950000",
                Logradouro = "Rua das Flores",
                Numero = "100",
                Bairro = "Centro",
                Cidade = "Teresópolis",
                Estado = "RJ"
            }
        };

        var resultado = await controller.Cadastrar(dados);

        Assert.IsType<CreatedAtActionResult>(resultado);

        var cliente = await banco.Context.Clientes
            .Include(c => c.Usuario)
            .Include(c => c.Endereco)
            .SingleAsync();

        Assert.Equal("João da Silva", cliente.Usuario!.Nome);
        Assert.Equal("Teresópolis", cliente.Endereco!.Cidade);

        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                "123456",
                cliente.Usuario.SenhaHash));
    }

    [Fact]
    public async Task NaoDeveCadastrarEmailRepetido()
    {
        using var banco = CriarBanco();

        banco.Context.Usuarios.Add(new Usuario
        {
            Nome = "Usuário existente",
            Email = "repetido@email.com",
            SenhaHash = "hash",
            Tipo = TipoUsuario.Cliente
        });

        await banco.Context.SaveChangesAsync();

        var controller = new ClientesController(
            banco.Context);

        var dados = new CriarClienteDto
        {
            Nome = "Outro usuário",
            Email = "repetido@email.com",
            Senha = "123456",
            CpfCnpj = "12345678901"
        };

        var resultado = await controller.Cadastrar(dados);

        Assert.IsType<ConflictObjectResult>(resultado);
    }

    [Fact]
    public async Task DeveCadastrarEquipamentoParaCliente()
    {
        using var banco = CriarBanco();

        var cliente = await CriarCliente(banco.Context);

        var controller = new EquipamentosController(
            banco.Context);

        var dados = new CriarEquipamentoDto
        {
            Tipo = "Notebook",
            Marca = "Dell",
            Modelo = "Inspiron",
            NumeroSerie = "SERIE-001"
        };

        var resultado = await controller.Cadastrar(
            cliente.Id,
            dados);

        Assert.IsType<CreatedAtActionResult>(resultado);

        var equipamento =
            await banco.Context.Equipamentos.SingleAsync();

        Assert.Equal(cliente.Id, equipamento.ClienteId);
        Assert.Equal("Inspiron", equipamento.Modelo);
    }

    [Fact]
    public async Task DeveRealizarLoginComSenhaCorreta()
    {
        using var banco = CriarBanco();

        var usuario = new Usuario
        {
            Nome = "Cliente Teste",
            Email = "cliente@email.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(
                "123456"),
            Tipo = TipoUsuario.Cliente
        };

        banco.Context.Usuarios.Add(usuario);
        await banco.Context.SaveChangesAsync();

        var controller = new AuthController(banco.Context);

        var resultado = await controller.Login(
            new LoginDto
            {
                Email = "cliente@email.com",
                Senha = "123456"
            });

        Assert.IsType<OkObjectResult>(resultado);
    }

    private static async Task<Cliente> CriarCliente(
        AppDbContext context)
    {
        var cliente = new Cliente
        {
            Usuario = new Usuario
            {
                Nome = "Cliente Teste",
                Email = "equipamento@email.com",
                SenhaHash = "hash",
                Tipo = TipoUsuario.Cliente
            },
            CpfCnpj = "12345678901"
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        return cliente;
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