using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Api;
using Api.Infra;
using Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1) CONFIGURAÇÃO DO JWT
// ============================================================
var jwt = builder.Configuration.GetSection(JwtOptions.Secao).Get<JwtOptions>() ?? new JwtOptions();

// A chave NÃO fica no repositório. Sem configuração, geramos uma chave
// efêmera (os tokens deixam de valer quando a API reinicia) e avisamos.
var chaveGerada = false;
if (string.IsNullOrWhiteSpace(jwt.Chave) || Encoding.UTF8.GetByteCount(jwt.Chave) < 32)
{
    jwt.Chave = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    chaveGerada = true;
}

var chaveAssinatura = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Chave));

// ============================================================
// 2) SERVIÇOS
// ============================================================
builder.Services.AddDbContext<AppDbContext>(opcoes =>
    opcoes.UseSqlite(builder.Configuration.GetConnectionString("Default")
                     ?? "Data Source=assistencia.db"));

builder.Services.AddSingleton<IOptions<JwtOptions>>(Options.Create(jwt));
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IAutenticacaoService, AutenticacaoService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtual>();

builder.Services.AddControllers();

// ============================================================
// 3) AUTENTICAÇÃO (JWT) E AUTORIZAÇÃO POR PERFIL
// ============================================================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        // Mantém os nomes de claim do token ("sub", "role", ...), sem o
        // mapeamento automático para as URIs do Schema da Microsoft.
        opcoes.MapInboundClaims = false;
        opcoes.SaveToken = false;

        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = chaveAssinatura,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimsCustomizados.Nome,
            RoleClaimType = ClaimsCustomizados.Perfil
        };

        opcoes.Events = new JwtBearerEvents
        {
            // 401 com JSON em vez de resposta vazia.
            OnChallenge = async contexto =>
            {
                contexto.HandleResponse();
                if (contexto.Response.HasStarted)
                    return;

                contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                contexto.Response.ContentType = "application/json; charset=utf-8";
                await contexto.Response.WriteAsJsonAsync(new
                {
                    erro = contexto.AuthenticateFailure is null
                        ? "Token de acesso não informado."
                        : "Token de acesso inválido ou expirado."
                });
            },

            // 403 com JSON (perfil autenticado, mas sem permissão).
            OnForbidden = async contexto =>
            {
                if (contexto.Response.HasStarted)
                    return;

                contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
                contexto.Response.ContentType = "application/json; charset=utf-8";
                await contexto.Response.WriteAsJsonAsync(new
                {
                    erro = "Seu perfil não tem permissão para executar esta operação."
                });
            }
        };
    });

builder.Services.AddAuthorization(opcoes =>
{
    // Tudo exige autenticação por padrão; o que for público leva [AllowAnonymous].
    opcoes.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    opcoes.AddPolicy(Politicas.SomenteAdministrador,
        politica => politica.RequireRole(Perfis.Administrador));

    opcoes.AddPolicy(Politicas.EquipeTecnica,
        politica => politica.RequireRole(Perfis.Tecnico, Perfis.Administrador));

    opcoes.AddPolicy(Politicas.SomenteCliente,
        politica => politica.RequireRole(Perfis.Cliente));
});

// ============================================================
// 4) CORS (origens vêm do appsettings)
// ============================================================
var origens = builder.Configuration.GetSection("Cors:Origens").Get<string[]>();

builder.Services.AddCors(opcoes => opcoes.AddDefaultPolicy(politica =>
{
    if (origens is { Length: > 0 })
        politica.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod();
    else
        politica.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
}));

// ============================================================
// 5) RATE LIMIT (proteção contra força bruta em login/cadastro)
// ============================================================
builder.Services.AddRateLimiter(opcoes =>
{
    opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Limite POR IP (partição por endereço remoto) — não um limite global.
    opcoes.AddPolicy(Politicas.RateLimitAuth, contexto =>
    {
        var ip = contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 15,                    // tentativas
            Window = TimeSpan.FromMinutes(1),    // por minuto
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    opcoes.OnRejected = async (contexto, ct) =>
    {
        if (contexto.HttpContext.Response.HasStarted)
            return;

        contexto.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        contexto.HttpContext.Response.ContentType = "application/json; charset=utf-8";
        await contexto.HttpContext.Response.WriteAsJsonAsync(new
        {
            erro = "Muitas tentativas. Aguarde um minuto e tente novamente."
        }, ct);
    };
});

var app = builder.Build();

// ============================================================
// 6) PIPELINE
// ============================================================
if (chaveGerada)
{
    app.Logger.LogWarning(
        "Jwt:Chave não configurada — usando chave efêmera (tokens invalidados ao reiniciar). " +
        "Defina com: dotnet user-secrets set \"Jwt:Chave\" \"<32+ caracteres>\" ou variável Jwt__Chave.");
}

app.UseMiddleware<TratamentoDeErrosMiddleware>();
app.UseRouting();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    api = "Assistência Técnica API",
    autenticacao = "JWT Bearer (Authorization: Bearer <token>)",
    perfis = new[] { Perfis.Cliente, Perfis.Tecnico, Perfis.Administrador },
    endpoints = new[]
    {
        "POST /api/auth/login",
        "POST /api/auth/refresh",
        "POST /api/auth/logout",
        "GET  /api/auth/eu",
        "PUT  /api/auth/senha",
        "POST /api/clientes",
        "GET  /api/clientes/me",
        "GET  /api/clientes",
        "GET  /api/clientes/{id}",
        "PUT  /api/clientes/me",
        "PUT  /api/clientes/{id}",
        "GET  /api/equipamentos/meus",
        "GET  /api/equipamentos",
        "POST /api/equipamentos",
        "GET  /api/equipamentos/{id}",
        "PUT  /api/equipamentos/{id}",
        "GET  /api/usuarios"
    }
})).AllowAnonymous();

app.MapControllers();

// ============================================================
// 7) BANCO DE DADOS + DADOS INICIAIS
// ============================================================
using (var escopo = app.Services.CreateScope())
{
    var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DbSeeder.ExecutarAsync(db, app.Configuration, app.Logger);
}

app.Run();
