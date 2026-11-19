using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class TrocarSenhaDto
{
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [StringLength(64, MinimumLength = 6, ErrorMessage = "A nova senha deve possuir entre 6 e 64 caracteres.")]
    public string NovaSenha { get; set; } = string.Empty;
}
