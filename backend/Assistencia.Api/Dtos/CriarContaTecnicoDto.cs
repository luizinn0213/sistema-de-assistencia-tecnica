using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class CriarContaTecnicoDto
{
    [Required(ErrorMessage = "O nome de exibição é obrigatório.")]
    public string NomeExibicao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve possuir pelo menos 6 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    public string? DescricaoProfissional { get; set; }

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    public string CidadeAtendimento { get; set; } = string.Empty;

    [Required(ErrorMessage = "O estado é obrigatório.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Informe a sigla do estado com duas letras.")]
    public string EstadoAtendimento { get; set; } = string.Empty;
}
