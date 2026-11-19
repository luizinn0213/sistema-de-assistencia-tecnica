using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class HistoricoOrcamento
{
    public const int TamanhoMaximoObservacao = 1000;

    public int Id { get; private set; }

    public int OrcamentoId { get; private set; }
    public Orcamento? Orcamento { get; private set; }

    public StatusOrcamento Status { get; private set; }

    public string Observacao { get; private set; }
        = string.Empty;

    public DateTime DataRegistro { get; private set; }

    // Utilizado pelo Entity Framework.
    private HistoricoOrcamento()
    {
    }

    internal HistoricoOrcamento(
        StatusOrcamento status,
        string observacao)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentException(
                "O status do histórico é inválido.");
        }

        if (string.IsNullOrWhiteSpace(observacao))
        {
            throw new ArgumentException(
                "A observação do histórico é obrigatória.");
        }

        if (observacao.Trim().Length >
            TamanhoMaximoObservacao)
        {
            throw new ArgumentException(
                $"A observação deve possuir no máximo " +
                $"{TamanhoMaximoObservacao} caracteres.");
        }

        Status = status;
        Observacao = observacao.Trim();
        DataRegistro = DateTime.UtcNow;
    }
}