using System.Text.Json.Serialization;

namespace Assistencia.Api.Models;

public class Equipamento
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    [JsonIgnore]
    public Cliente? Cliente { get; set; }

    public string Tipo { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
}
