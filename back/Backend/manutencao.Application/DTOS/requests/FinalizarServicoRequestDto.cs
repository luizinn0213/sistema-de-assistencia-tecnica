namespace Manutencao.Application.DTOs.Requests;

public class FinalizarServicoRequestDto
{
    public int SolicitacaoId { get; set; }
    public int TecnicoId { get; set; }
    public string SolucaoAplicada { get; set; } = string.Empty;
    public string? ObservacaoFinal { get; set; }
}