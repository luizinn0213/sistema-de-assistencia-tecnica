namespace Manutencao.Domain.Entities;

public class Tecnico
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<Solicitacao> Solicitacoes { get; set; } = new List<Solicitacao>();
}