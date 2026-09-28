using Assistencia.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class AdicionarEspecialidadeTecnicoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "A especialidade é obrigatória.")]
    public int EspecialidadeId { get; set; }

    [EnumDataType(typeof(NivelExperiencia))]
    public NivelExperiencia NivelExperiencia { get; set; }

    [Range(0, 80, ErrorMessage = "Os anos de experiência devem estar entre 0 e 80.")]
    public int AnosExperiencia { get; set; }
}