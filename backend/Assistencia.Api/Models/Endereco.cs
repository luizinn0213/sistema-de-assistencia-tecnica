using System.Text.Json.Serialization;

namespace Assistencia.Api.Models;

public class Endereco
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    [JsonIgnore]
    public Cliente? Cliente { get; set; }

    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
