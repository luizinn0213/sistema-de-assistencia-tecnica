using Manutencao.Domain.Enums;

namespace Manutencao.Domain.Entities;

public class Solicitacao
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public StatusSolicitacao Status { get; set; } = StatusSolicitacao.Aberta;

    public int TecnicoId { get; set; }
    public Tecnico? Tecnico { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataFinalizacao { get; set; }
    public string? SolucaoAplicada { get; set; }

    public ICollection<HistoricoSolicitacao> Historicos { get; set; } = new List<HistoricoSolicitacao>();
}