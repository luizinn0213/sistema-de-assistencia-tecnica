using Assistencia.Api.Enums;

namespace Assistencia.Api.Dtos.Matching;

public class ContextoPontuacaoMatching
{
    public bool PossuiEspecialidade { get; init; }
    public NivelExperiencia NivelExperiencia { get; init; }
    public int AnosExperiencia { get; init; }
    public bool MesmaCidade { get; init; }
}
