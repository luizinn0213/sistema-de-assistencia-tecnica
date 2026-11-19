using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class CriarAvaliacaoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "A execução é obrigatória.")]
    public int ExecucaoServicoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "O cliente é obrigatório.")]
    public int ClienteId { get; set; }

    [Range(1, 5, ErrorMessage = "A nota deve estar entre 1 e 5.")]
    public int Nota { get; set; }

    [StringLength(1000, ErrorMessage = "O comentário deve possuir no máximo 1000 caracteres.")]
    public string? Comentario { get; set; }
}
