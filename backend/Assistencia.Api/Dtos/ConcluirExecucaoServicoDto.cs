using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class ConcluirExecucaoServicoDto
{
    [StringLength(1000, ErrorMessage = "A observação final deve possuir no máximo 1000 caracteres.")]
    public string? ObservacaoFinal { get; set; }
}
