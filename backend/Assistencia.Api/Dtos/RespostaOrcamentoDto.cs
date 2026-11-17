using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class RespostaOrcamentoDto
{
    [StringLength(
        900,
        ErrorMessage =
            "A observação deve possuir no máximo 900 caracteres.")]
    public string Observacao { get; set; }
        = string.Empty;
}