using Manutencao.Domain.Enums;

namespace Manutencao.Application.DTOs.Requests;

public class RegistrarHistoricoRequestDto
{
    public int SolicitacaoId { get; set; }
    public int TecnicoId { get; set; }
    public TipoHistorico Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
}