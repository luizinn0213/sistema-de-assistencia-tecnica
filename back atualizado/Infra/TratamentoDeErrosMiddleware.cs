namespace Api.Infra;

/// <summary>
/// Converte qualquer exceção não tratada em um 500 com JSON padronizado,
/// evitando que o cliente receba HTML de erro ou stack trace.
/// </summary>
public sealed class TratamentoDeErrosMiddleware
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<TratamentoDeErrosMiddleware> _log;

    public TratamentoDeErrosMiddleware(RequestDelegate proximo, ILogger<TratamentoDeErrosMiddleware> log)
    {
        _proximo = proximo;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (Exception ex)
        {
            if (contexto.Response.HasStarted)
                throw;

            _log.LogError(ex, "Erro não tratado em {Metodo} {Caminho}",
                contexto.Request.Method, contexto.Request.Path);

            contexto.Response.Clear();
            contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
            contexto.Response.ContentType = "application/json; charset=utf-8";

            await contexto.Response.WriteAsJsonAsync(new
            {
                erro = "Erro interno no servidor.",
                detalhe = contexto.RequestServices
                    .GetRequiredService<IHostEnvironment>()
                    .IsDevelopment() ? ex.Message : null
            });
        }
    }
}
