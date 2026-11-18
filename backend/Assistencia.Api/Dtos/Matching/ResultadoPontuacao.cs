namespace Assistencia.Api.Dtos.Matching;

public class ResultadoPontuacao
{
    public int Pontos { get; init; }
    public string? Destaque { get; init; }

    public ResultadoPontuacao(int pontos, string? destaque = null)
    {
        Pontos = pontos;
        Destaque = destaque;
    }
}
