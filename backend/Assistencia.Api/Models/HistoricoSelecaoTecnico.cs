using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class HistoricoSelecaoTecnico
{
    public int Id { get; private set; }

    public int SelecaoTecnicoId { get; private set; }
    public SelecaoTecnico? SelecaoTecnico { get; private set; }

    public StatusSelecaoTecnico Status { get; private set; }
    public string Observacao { get; private set; } = string.Empty;
    public DateTime DataRegistro { get; private set; }

    // Utilizado pelo Entity Framework
    public HistoricoSelecaoTecnico()
    {
    }

    public HistoricoSelecaoTecnico(
        StatusSelecaoTecnico status,
        string observacao)
    {
        Status = status;
        Observacao = observacao;
        DataRegistro = DateTime.UtcNow;
    }
}