using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class SelecionarTecnicoDto
{
    [Range(1, int.MaxValue)]
    public int SolicitacaoId { get; set; }

    [Range(1, int.MaxValue)]
    public int TecnicoId { get; set; }

    [Range(1, int.MaxValue)]
    public int EspecialidadeId { get; set; }
}