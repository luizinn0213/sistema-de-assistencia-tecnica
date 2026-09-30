namespace Assistencia.Api.Dtos;

public class CriarClienteDto
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string CpfCnpj { get; set; } = string.Empty;
    public EnderecoDto? Endereco { get; set; }
}
