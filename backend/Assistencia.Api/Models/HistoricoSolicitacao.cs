using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class HistoricoSolicitacao
{
    public int Id { get; private set; }

    public int SolicitacaoId { get; private set; }
    public Solicitacao? Solicitacao { get; private set; }

    public StatusSolicitacao StatusAnterior { get; private set; }
    public StatusSolicitacao NovoStatus { get; private set; }

    public DateTime DataAlteracao { get; private set; }

    public HistoricoSolicitacao()
    {
    }

    public HistoricoSolicitacao(
        Solicitacao solicitacao,
        StatusSolicitacao statusAnterior,
        StatusSolicitacao novoStatus)
    {
        ArgumentNullException.ThrowIfNull(solicitacao);

        if (statusAnterior == novoStatus)
            throw new ArgumentException("Os status devem ser diferentes.");

        Solicitacao = solicitacao;
        StatusAnterior = statusAnterior;
        NovoStatus = novoStatus;
        DataAlteracao = DateTime.UtcNow;
    }
}