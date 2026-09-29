using System.Text.Json.Serialization;
using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class Pagamento
{
    public int Id { get; set; }
    public int OrcamentoId { get; set; }
    public decimal Valor { get; set; }

    public string FormaPagamento { get; set; }
        = "Pix";

    public StatusPagamento Status { get; set; }
        = StatusPagamento.Pendente;

    public DateTime? DataPagamento { get; set; }

    [JsonIgnore]
    public Orcamento? Orcamento { get; set; }
}