using Assistencia.Api.Dtos.Matching;

namespace Assistencia.Api.Services.Contracts;

public interface ICriterioPontuacaoMatching
{
    ResultadoPontuacao Calcular(ContextoPontuacaoMatching contexto);
}
