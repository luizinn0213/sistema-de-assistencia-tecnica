using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class ItemOrcamento
{
    public int Id { get; set; }
    public int OrcamentoId { get; set; }

    public string Descricao { get; set; }
        = string.Empty;

    public TipoItemOrcamento Tipo { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }

    [NotMapped]
    public decimal ValorTotal
        => Quantidade * ValorUnitario;

    [JsonIgnore]
    public Orcamento? Orcamento { get; set; }
}