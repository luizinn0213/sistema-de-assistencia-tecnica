using Assistencia.Api.Data;
using Microsoft.EntityFrameworkCore;
using Assistencia.Api.Repositories;
using Assistencia.Api.Repositories.Contracts;
using Assistencia.Api.Services;
using Assistencia.Api.Services.Contracts;
using Assistencia.Api.Filters;
using Assistencia.Api.Security;

var builder = WebApplication.CreateBuilder(args);

// Registra os controllers da API.
builder.Services.AddControllers(opcoes =>
{
    opcoes.Filters.Add<ApiExceptionFilter>();
});

// Registra a documentação OpenAPI.
builder.Services.AddOpenApi();

builder.Services.AddScoped<
    IDiagnosticoRepository,
    DiagnosticoRepository>();

builder.Services.AddScoped<
    ISelecaoTecnicoRepository,
    SelecaoTecnicoRepository>();

builder.Services.AddScoped<
    IDiagnosticoService,
    DiagnosticoService>();

builder.Services.AddScoped<
    IOrcamentoRepository,
    OrcamentoRepository>();
    
builder.Services.AddScoped<
    IOrcamentoService,
    OrcamentoService>();    
    
builder.Services.AddScoped<
    IExecucaoServicoRepository,
    ExecucaoServicoRepository>();

builder.Services.AddScoped<
    IAvaliacaoRepository,
    AvaliacaoRepository>();

builder.Services.AddScoped<
    IExecucaoServicoService,
    ExecucaoServicoService>();

builder.Services.AddScoped<
    IAvaliacaoService,
    AvaliacaoService>();

// Registra o Entity Framework usando SQLite.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AdicionarAutenticacaoJwt(builder.Configuration);

var app = builder.Build();

// Disponibiliza a documentação durante o desenvolvimento.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();