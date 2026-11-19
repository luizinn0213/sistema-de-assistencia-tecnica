using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class IniciarExecucaoServicoDto
{
    [StringLength(1000, ErrorMessage = "A observação deve possuir no máximo 1000 caracteres.")]
    public string? Observacao { get; set; }
}
