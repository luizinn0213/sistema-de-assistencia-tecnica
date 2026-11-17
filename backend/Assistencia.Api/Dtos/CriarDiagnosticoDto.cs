using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class CriarDiagnosticoDto
{
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "A solicitação é obrigatória.")]
    public int SolicitacaoId { get; set; }

    [Required(
        ErrorMessage =
            "A descrição do defeito é obrigatória.")]
    [StringLength(
        2000,
        ErrorMessage =
            "A descrição deve possuir no máximo 2000 caracteres.")]
    public string DescricaoDefeito { get; set; }
        = string.Empty;
}