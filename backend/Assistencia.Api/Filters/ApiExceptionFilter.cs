using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Assistencia.Api.Filters;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(
        ExceptionContext context)
    {
        var resultado = context.Exception switch
        {
            ArgumentException erro => CriarResultado(
                StatusCodes.Status400BadRequest,
                erro.Message),

            KeyNotFoundException erro => CriarResultado(
                StatusCodes.Status404NotFound,
                erro.Message),

            InvalidOperationException erro => CriarResultado(
                StatusCodes.Status409Conflict,
                erro.Message),

            _ => null
        };

        if (resultado is null)
        {
            return;
        }

        context.Result = resultado;
        context.ExceptionHandled = true;
    }

    private static ObjectResult CriarResultado(
        int status,
        string mensagem)
    {
        return new ObjectResult(
            new { erro = mensagem })
        {
            StatusCode = status
        };
    }
}