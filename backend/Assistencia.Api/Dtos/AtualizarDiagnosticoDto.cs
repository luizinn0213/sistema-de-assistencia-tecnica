using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class AtualizarDiagnosticoDto
{
    [Required(
        ErrorMessage =
            "A descrição do defeito é obrigatória.")]
    [StringLength(
        2000,
        ErrorMessage =
            "A descrição deve possuir no máximo 2000 caracteres.")]
    public string DescricaoDefeito { get; set; }
        = string.Empty;

    [StringLength(
        2000,
        ErrorMessage =
            "A solução deve possuir no máximo 2000 caracteres.")]
    public string SolucaoRecomendada { get; set; }
        = string.Empty;
}