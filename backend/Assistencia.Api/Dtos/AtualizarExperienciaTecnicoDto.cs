using Assistencia.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class AtualizarExperienciaTecnicoDto
{
    [EnumDataType(typeof(NivelExperiencia))]
    public NivelExperiencia NivelExperiencia { get; set; }

    [Range(
        0,
        80,
        ErrorMessage = "Os anos de experiência devem estar entre 0 e 80.")]
    public int AnosExperiencia { get; set; }
}