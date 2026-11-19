using System.IdentityModel.Tokens.Jwt;
using Assistencia.Api.Enums;
using Assistencia.Api.Models;
using Assistencia.Api.Security;
using Microsoft.Extensions.Options;

namespace Assistencia.Api.Tests;

public class TokenServiceTests
{
    private readonly TokenService _service = new(
        Options.Create(new JwtOptions
        {
            Chave = "chave-de-testes-com-mais-de-trinta-e-dois-bytes",
            ExpiraMinutos = 30,
            RefreshDias = 7
        }));

    [Fact]
    public void DeveGerarAccessTokenComClaims()
    {
        var usuario = new Usuario
        {
            Id = 7,
            Nome = "Cliente Teste",
            Email = "cliente@teste.com",
            Tipo = TipoUsuario.Cliente
        };

        var resultado = _service.GerarAccessToken(usuario, 3);
        var token = new JwtSecurityTokenHandler()
            .ReadJwtToken(resultado.Token);

        Assert.Contains(token.Claims, c => c.Type == ClaimsJwt.UsuarioId && c.Value == "7");
        Assert.Contains(token.Claims, c => c.Type == ClaimsJwt.Perfil && c.Value == "Cliente");
        Assert.Contains(token.Claims, c => c.Type == ClaimsJwt.ClienteId && c.Value == "3");
        Assert.True(resultado.ExpiraEm > DateTime.UtcNow);
    }

    [Fact]
    public void DeveGerarRefreshTokenSemGuardarTextoPuro()
    {
        var resultado = _service.GerarRefreshToken(1);

        Assert.NotEmpty(resultado.Token);
        Assert.NotEqual(resultado.Token, resultado.Registro.TokenHash);
        Assert.Equal(
            _service.CalcularHash(resultado.Token),
            resultado.Registro.TokenHash);
    }

    [Fact]
    public void HashDeveSerDeterministico()
    {
        var primeiro = _service.CalcularHash("token-teste");
        var segundo = _service.CalcularHash("token-teste");

        Assert.Equal(primeiro, segundo);
    }
}
