namespace Manutencao.Application.DTOs.Responses;

public class SolicitacaoResponseDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TecnicoId { get; set; }
    public string TecnicoNome { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public string? SolucaoAplicada { get; set; }
}