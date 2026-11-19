using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class HistoricoExecucaoServico
{
    public const int TamanhoMaximoObservacao = 1000;

    public int Id { get; private set; }
    public int ExecucaoServicoId { get; private set; }
    public ExecucaoServico? ExecucaoServico { get; private set; }
    public StatusExecucaoServico Status { get; private set; }
    public string Observacao { get; private set; } = string.Empty;
    public DateTime DataRegistro { get; private set; }

    private HistoricoExecucaoServico()
    {
    }

    internal HistoricoExecucaoServico(
        StatusExecucaoServico status,
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

        var texto = observacao.Trim();

        if (texto.Length > TamanhoMaximoObservacao)
        {
            throw new ArgumentException(
                $"A observação deve possuir no máximo " +
                $"{TamanhoMaximoObservacao} caracteres.");
        }

        Status = status;
        Observacao = texto;
        DataRegistro = DateTime.UtcNow;
    }
}
