using Assistencia.Api.Dtos.Matching;
using Assistencia.Api.Services.Contracts;

namespace Assistencia.Api.Services.Matching;

public class CriterioExperiencia : ICriterioPontuacaoMatching
{
    public ResultadoPontuacao Calcular(ContextoPontuacaoMatching contexto)
    {
        var pontosNivel = (int)contexto.NivelExperiencia * 10;
        var pontosAnos = Math.Min(Math.Max(contexto.AnosExperiencia, 0), 10);
        var totalPontos = pontosNivel + pontosAnos;

        string? destaque = null;
        if (contexto.AnosExperiencia > 0)
        {
            destaque = $"{contexto.AnosExperiencia} {(contexto.AnosExperiencia == 1 ? "ano" : "anos")} de experiência ({contexto.NivelExperiencia})";
        }
        else if (pontosNivel > 0)
        {
            destaque = $"Nível {contexto.NivelExperiencia}";
        }

        return new ResultadoPontuacao(totalPontos, destaque);
    }
}
