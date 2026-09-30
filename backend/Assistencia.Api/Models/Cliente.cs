namespace Assistencia.Api.Models;

public class Cliente
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string CpfCnpj { get; set; } = string.Empty;

    public Endereco? Endereco { get; set; }

    public ICollection<Equipamento> Equipamentos { get; set; }
        = new List<Equipamento>();
}
