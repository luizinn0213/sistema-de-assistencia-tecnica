using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class CriarOrcamentoDto
{
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "O diagnóstico é obrigatório.")]
    public int DiagnosticoId { get; set; }
}