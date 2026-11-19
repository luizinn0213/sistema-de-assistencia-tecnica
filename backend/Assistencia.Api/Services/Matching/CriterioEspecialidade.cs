using Assistencia.Api.Dtos.Matching;
using Assistencia.Api.Services.Contracts;

namespace Assistencia.Api.Services.Matching;

public class CriterioEspecialidade : ICriterioPontuacaoMatching
{
    public ResultadoPontuacao Calcular(ContextoPontuacaoMatching contexto)
    {
        if (!contexto.PossuiEspecialidade)
        {
            return new ResultadoPontuacao(0);
        }

        return new ResultadoPontuacao(50, "Especialidade compatível");
    }
}
