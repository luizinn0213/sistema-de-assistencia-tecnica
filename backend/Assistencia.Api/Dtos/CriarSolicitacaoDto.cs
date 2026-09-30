using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class CriarSolicitacaoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "O equipamento é obrigatório.")]
    public int EquipamentoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A especialidade é obrigatória.")]
    public int EspecialidadeId { get; set; }

    [Required(ErrorMessage = "A descrição do problema é obrigatória.")]
    [StringLength(
        1000,
        ErrorMessage = "A descrição do problema deve possuir no máximo 1000 caracteres.")]
    public string DescricaoProblema { get; set; } = string.Empty;
}
