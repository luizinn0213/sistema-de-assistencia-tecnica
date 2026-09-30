using Manutencao.Domain.Enums;

namespace Manutencao.Domain.Entities;

public class HistoricoSolicitacao
{
    public int Id { get; set; }

    public int SolicitacaoId { get; set; }
    public Solicitacao? Solicitacao { get; set; }

    public int TecnicoId { get; set; }
    public Tecnico? Tecnico { get; set; }

    public TipoHistorico Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
}