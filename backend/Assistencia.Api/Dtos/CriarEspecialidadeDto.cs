using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class CriarEspecialidadeDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;
}