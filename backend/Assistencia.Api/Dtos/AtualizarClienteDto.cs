namespace Assistencia.Api.Dtos;

public class AtualizarClienteDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public EnderecoDto? Endereco { get; set; }
}
