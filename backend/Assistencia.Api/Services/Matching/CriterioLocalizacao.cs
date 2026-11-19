using Assistencia.Api.Dtos.Matching;
using Assistencia.Api.Services.Contracts;

namespace Assistencia.Api.Services.Matching;

public class CriterioLocalizacao : ICriterioPontuacaoMatching
{
    public ResultadoPontuacao Calcular(ContextoPontuacaoMatching contexto)
    {
        if (!contexto.MesmaCidade)
        {
            return new ResultadoPontuacao(0);
        }

        return new ResultadoPontuacao(30, "Atende na mesma cidade");
    }
}
