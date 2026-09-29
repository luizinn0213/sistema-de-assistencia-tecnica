namespace Api;

public enum TipoUsuario { Cliente = 1, Tecnico = 2, Administrador = 3 }

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string SenhaHash { get; set; } = "";
    public string? Telefone { get; set; }
    public TipoUsuario Tipo { get; set; } = TipoUsuario.Cliente;
}

public class Cliente
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public string CpfCnpj { get; set; } = "";
    public Endereco? Endereco { get; set; }
    public List<Equipamento> Equipamentos { get; set; } = new();
}

public class Endereco
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Cep { get; set; } = "";
    public string Logradouro { get; set; } = "";
    public string Numero { get; set; } = "";
    public string Bairro { get; set; } = "";
    public string Cidade { get; set; } = "";
    public string Estado { get; set; } = "";
}

public class Equipamento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public string Tipo { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string NumeroSerie { get; set; } = "";
}