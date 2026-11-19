using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class CriarExecucaoServicoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "O orçamento é obrigatório.")]
    public int OrcamentoId { get; set; }
}
