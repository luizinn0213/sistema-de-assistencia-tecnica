using Assistencia.Api.Dtos;
using Assistencia.Api.Enums;
using Assistencia.Api.Models;
using Assistencia.Api.Repositories.Contracts;
using Assistencia.Api.Security;
using Assistencia.Api.Services;
using Microsoft.Extensions.Options;

namespace Assistencia.Api.Tests;

public class AutenticacaoServiceTests
{
    [Fact]
    public async Task DeveRealizarLoginEGuardarRefreshToken()
    {
        var repository = new AutenticacaoRepositoryFake();
        var service = CriarService(repository);

        var resposta = await service.LoginAsync(new LoginDto
        {
            Email = "CLIENTE@TESTE.COM",
            Senha = "Senha@123"
        });

        Assert.NotEmpty(resposta.AccessToken);
        Assert.NotEmpty(resposta.RefreshToken);
        Assert.Equal("cliente@teste.com", resposta.Usuario.Email);
        Assert.Equal(2, resposta.Usuario.ClienteId);
        Assert.Single(repository.Tokens);
        Assert.True(repository.AlteracoesSalvas);
    }

    [Fact]
    public async Task NaoDeveRealizarLoginComSenhaInvalida()
    {
        var service = CriarService(new AutenticacaoRepositoryFake());

        var erro = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.LoginAsync(new LoginDto
            {
                Email = "cliente@teste.com",
                Senha = "senha-errada"
            }));

        Assert.Equal("E-mail ou senha inválidos.", erro.Message);
    }

    [Fact]
    public async Task DeveRevogarTokensNoLogout()
    {
        var repository = new AutenticacaoRepositoryFake();
        var service = CriarService(repository);
        await service.LoginAsync(new LoginDto
        {
            Email = "cliente@teste.com",
            Senha = "Senha@123"
        });

        await service.LogoutAsync(1);

        Assert.All(repository.Tokens, token => Assert.False(token.Ativo));
    }

    [Fact]
    public async Task DeveTrocarSenhaERevogarSessoes()
    {
        var repository = new AutenticacaoRepositoryFake();
        var service = CriarService(repository);
        await service.LoginAsync(new LoginDto
        {
            Email = "cliente@teste.com",
            Senha = "Senha@123"
        });

        await service.TrocarSenhaAsync(1, new TrocarSenhaDto
        {
            SenhaAtual = "Senha@123",
            NovaSenha = "NovaSenha@456"
        });

        Assert.True(BCrypt.Net.BCrypt.Verify(
            "NovaSenha@456",
            repository.Usuario.SenhaHash));
        Assert.All(repository.Tokens, token => Assert.False(token.Ativo));
    }

    private static AutenticacaoService CriarService(
        AutenticacaoRepositoryFake repository)
    {
        var tokenService = new TokenService(
            Options.Create(new JwtOptions
            {
                Chave = "chave-de-testes-com-mais-de-trinta-e-dois-bytes",
                ExpiraMinutos = 30,
                RefreshDias = 7
            }));

        return new AutenticacaoService(repository, tokenService);
    }

    private sealed class AutenticacaoRepositoryFake
        : IAutenticacaoRepository
    {
        public Usuario Usuario { get; } = new()
        {
            Id = 1,
            Nome = "Cliente Teste",
            Email = "cliente@teste.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("Senha@123"),
            Tipo = TipoUsuario.Cliente
        };

        public List<RefreshToken> Tokens { get; } = new();
        public bool AlteracoesSalvas { get; private set; }

        public Task<Usuario?> ObterUsuarioPorEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Usuario?>(
                email == Usuario.Email ? Usuario : null);
        }

        public Task<Usuario?> ObterUsuarioPorIdAsync(
            int usuarioId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Usuario?>(
                usuarioId == Usuario.Id ? Usuario : null);
        }

        public Task<int?> ObterClienteIdAsync(
            int usuarioId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<int?>(2);
        }

        public Task<RefreshToken?> ObterRefreshTokenAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Tokens.FirstOrDefault(t => t.TokenHash == tokenHash));
        }

        public Task AdicionarRefreshTokenAsync(
            RefreshToken refreshToken,
            CancellationToken cancellationToken = default)
        {
            Tokens.Add(refreshToken);
            return Task.CompletedTask;
        }

        public Task RevogarTokensAtivosAsync(
            int usuarioId,
            CancellationToken cancellationToken = default)
        {
            foreach (var token in Tokens.Where(t => t.Ativo))
            {
                token.Revogar();
            }

            return Task.CompletedTask;
        }

        public Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default)
        {
            AlteracoesSalvas = true;
            return Task.CompletedTask;
        }
    }
}
