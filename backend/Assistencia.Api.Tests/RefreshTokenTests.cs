using Assistencia.Api.Models;

namespace Assistencia.Api.Tests;

public class RefreshTokenTests
{
    [Fact]
    public void DeveCriarTokenAtivo()
    {
        var token = new RefreshToken(
            1,
            "hash-valido",
            DateTime.UtcNow.AddDays(1));

        Assert.True(token.Ativo);
        Assert.Null(token.DataRevogacao);
    }

    [Fact]
    public void DeveRevogarToken()
    {
        var token = new RefreshToken(
            1,
            "hash-valido",
            DateTime.UtcNow.AddDays(1));

        token.Revogar("novo-hash");

        Assert.False(token.Ativo);
        Assert.NotNull(token.DataRevogacao);
        Assert.Equal("novo-hash", token.SubstituidoPorHash);
    }

    [Fact]
    public void NaoDeveCriarTokenExpirado()
    {
        Assert.Throws<ArgumentException>(() => new RefreshToken(
            1,
            "hash-valido",
            DateTime.UtcNow.AddMinutes(-1)));
    }
}
