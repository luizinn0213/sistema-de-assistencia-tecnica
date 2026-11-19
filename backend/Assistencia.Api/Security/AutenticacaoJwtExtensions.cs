using System.Security.Cryptography;
using System.Text;
using Assistencia.Api.Repositories;
using Assistencia.Api.Repositories.Contracts;
using Assistencia.Api.Services;
using Assistencia.Api.Services.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Assistencia.Api.Security;

public static class AutenticacaoJwtExtensions
{
    public static IServiceCollection AdicionarAutenticacaoJwt(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var opcoes = configuration
            .GetSection(JwtOptions.Secao)
            .Get<JwtOptions>() ?? new JwtOptions();

        if (Encoding.UTF8.GetByteCount(opcoes.Chave) < 32)
        {
            opcoes.Chave = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(48));
        }

        services.AddSingleton<IOptions<JwtOptions>>(
            Options.Create(opcoes));
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<IAutenticacaoRepository, AutenticacaoRepository>();
        services.AddScoped<IAutenticacaoService, AutenticacaoService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUsuarioAtual, UsuarioAtual>();

        var chave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(opcoes.Chave));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(configuracao =>
            {
                configuracao.MapInboundClaims = false;
                configuracao.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = opcoes.Issuer,
                        ValidateAudience = true,
                        ValidAudience = opcoes.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = chave,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30),
                        NameClaimType = ClaimsJwt.Nome,
                        RoleClaimType = ClaimsJwt.Perfil
                    };

                configuracao.Events = new JwtBearerEvents
                {
                    OnChallenge = async contexto =>
                    {
                        contexto.HandleResponse();

                        if (contexto.Response.HasStarted)
                        {
                            return;
                        }

                        contexto.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;
                        contexto.Response.ContentType =
                            "application/json; charset=utf-8";
                        await contexto.Response.WriteAsJsonAsync(new
                        {
                            erro = "Token não informado, inválido ou expirado."
                        });
                    },
                    OnForbidden = async contexto =>
                    {
                        contexto.Response.StatusCode =
                            StatusCodes.Status403Forbidden;
                        contexto.Response.ContentType =
                            "application/json; charset=utf-8";
                        await contexto.Response.WriteAsJsonAsync(new
                        {
                            erro = "Seu perfil não possui permissão para essa operação."
                        });
                    }
                };
            });

        services.AddAuthorization(autorizacao =>
        {
            autorizacao.AddPolicy(
                PoliticasAutorizacao.SomenteCliente,
                politica => politica.RequireRole(
                    PoliticasAutorizacao.Cliente));

            autorizacao.AddPolicy(
                PoliticasAutorizacao.EquipeTecnica,
                politica => politica.RequireRole(
                    PoliticasAutorizacao.Tecnico,
                    PoliticasAutorizacao.Administrador));

            autorizacao.AddPolicy(
                PoliticasAutorizacao.SomenteAdministrador,
                politica => politica.RequireRole(
                    PoliticasAutorizacao.Administrador));
        });
        return services;
    }
}
