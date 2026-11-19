using Assistencia.Api.Enums;

namespace Assistencia.Api.Dtos.Matching;

public class TecnicoRankingDto
{
    public int Id { get; init; }
    public string NomeExibicao { get; init; } = string.Empty;
    public string DescricaoProfissional { get; init; } = string.Empty;
    public string CidadeAtendimento { get; init; } = string.Empty;
    public string EstadoAtendimento { get; init; } = string.Empty;
    public int EspecialidadeId { get; init; }
    public string Especialidade { get; init; } = string.Empty;
    public NivelExperiencia NivelExperiencia { get; init; }
    public int AnosExperiencia { get; init; }
    public bool MesmaCidade { get; init; }
    public int Pontuacao { get; init; }
    public List<string> Destaques { get; init; } = new();
}
