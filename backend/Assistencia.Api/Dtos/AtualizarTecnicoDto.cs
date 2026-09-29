using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class AtualizarTecnicoDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string NomeExibicao { get; set; } = string.Empty;

    public string DescricaoProfissional { get; set; } =
        string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    public string CidadeAtendimento { get; set; } =
        string.Empty;

    [Required(ErrorMessage = "O estado é obrigatório.")]
    [StringLength(
        2,
        MinimumLength = 2,
        ErrorMessage = "O estado deve possuir duas letras.")]
    public string EstadoAtendimento { get; set; } =
        string.Empty;
}