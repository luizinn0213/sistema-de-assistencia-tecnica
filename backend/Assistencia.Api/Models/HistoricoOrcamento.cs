using System.Text.Json.Serialization;
using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class HistoricoOrcamento
{
    public int Id { get; set; }
    public int OrcamentoId { get; set; }
    public int Versao { get; set; }
    public decimal ValorTotal { get; set; }

    public StatusOrcamento Status { get; set; }

    public DateTime DataRegistro { get; set; }
        = DateTime.UtcNow;

    public string Obs { get; set; }
        = string.Empty;

    [JsonIgnore]
    public Orcamento? Orcamento { get; set; }
}