namespace Manutencao.Application.DTOs.Responses;

public class HistoricoResponseDto
{
    public int Id { get; set; }
    public int SolicitacaoId { get; set; }
    public int TecnicoId { get; set; }
    public string TecnicoNome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataRegistro { get; set; }
}