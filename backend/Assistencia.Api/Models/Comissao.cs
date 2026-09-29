using System.Text.Json.Serialization;

namespace Assistencia.Api.Models;

public class Comissao
{
    public int Id { get; set; }
    public int OrcamentoId { get; set; }

    public decimal Porcentagem { get; set; }
        = 10.0m;

    public decimal ValorComissao { get; set; }

    public DateTime DataCalculo { get; set; }
        = DateTime.UtcNow;

    [JsonIgnore]
    public Orcamento? Orcamento { get; set; }
}