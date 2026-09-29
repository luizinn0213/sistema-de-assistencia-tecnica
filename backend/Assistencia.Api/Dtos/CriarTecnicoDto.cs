using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class CriarTecnicoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "O usuário é obrigatório.")]
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string NomeExibicao { get; set; } = string.Empty;

    public string DescricaoProfissional { get; set; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    public string CidadeAtendimento { get; set; } = string.Empty;

    [Required(ErrorMessage = "O estado é obrigatório.")]
    public string EstadoAtendimento { get; set; } = string.Empty;
}