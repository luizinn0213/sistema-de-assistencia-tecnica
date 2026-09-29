using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class Orcamento
{
    public int Id { get; set; }
    public int DiagnosticoId { get; set; }
    public bool DiagnosticoConcluido { get; set; }
    public int Versao { get; set; } = 1;

    public StatusOrcamento Status { get; set; }
        = StatusOrcamento.Pendente;

    public DateTime DataCriacao { get; set; }
        = DateTime.UtcNow;

    public DateTime? DataResposta { get; set; }
    public decimal ValorTotal { get; set; }

    public List<ItemOrcamento> Itens { get; set; }
        = new();

    public List<HistoricoOrcamento> Historico { get; set; }
        = new();

    public Pagamento? Pagamento { get; set; }
    public Comissao? Comissao { get; set; }
}