using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class RefreshTokenDto
{
    [Required(ErrorMessage = "O refresh token é obrigatório.")]
    public string RefreshToken { get; set; } = string.Empty;
}
