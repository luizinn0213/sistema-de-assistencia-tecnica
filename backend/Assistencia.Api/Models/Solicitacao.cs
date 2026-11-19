using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class Solicitacao
{
    public const int TamanhoMaximoDescricao = 1000;

    public int Id { get; private set; }
    public string Numero { get; private set; } = string.Empty;
    public string DescricaoProblema { get; private set; } = string.Empty;
    public StatusSolicitacao Status { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public int ClienteId { get; private set; }
    public Cliente? Cliente { get; private set; }

    public int EquipamentoId { get; private set; }
    public Equipamento? Equipamento { get; private set; }

    public int EspecialidadeId { get; private set; }
    public Especialidade? Especialidade { get; private set; }

    public ICollection<HistoricoSolicitacao> Historico
        { get; private set; } = new List<HistoricoSolicitacao>();

    public Solicitacao()
    {
    }

    public Solicitacao(
        int clienteId,
        Equipamento equipamento,
        Especialidade especialidade,
        string descricaoProblema)
    {
        if (clienteId <= 0)
            throw new ArgumentException(
                "O cliente é obrigatório.");

        ArgumentNullException.ThrowIfNull(equipamento);
        ArgumentNullException.ThrowIfNull(especialidade);

        if (equipamento.ClienteId != clienteId)
        {
            throw new InvalidOperationException(
                "O equipamento não pertence ao cliente.");
        }

        if (!especialidade.Ativa)
        {
            throw new InvalidOperationException(
                "A especialidade está inativa.");
        }

        var descricao = descricaoProblema?.Trim();

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException(
                "A descrição do problema é obrigatória.");

        if (descricao.Length > TamanhoMaximoDescricao)
            throw new ArgumentException(
                $"A descrição do problema deve possuir no máximo {TamanhoMaximoDescricao} caracteres.");

        ClienteId = clienteId;

        Equipamento = equipamento;
        EquipamentoId = equipamento.Id;

        Especialidade = especialidade;
        EspecialidadeId = especialidade.Id;

        DescricaoProblema = descricao;
        Numero = GerarNumero();
        Status = StatusSolicitacao.Aberta;
        DataCriacao = DateTime.UtcNow;
    }

    public void AlterarStatus(StatusSolicitacao novoStatus)
    {
        if (Status == StatusSolicitacao.Cancelada ||
            Status == StatusSolicitacao.Concluida)
        {
            throw new InvalidOperationException(
                "Não é possível alterar uma solicitação encerrada.");
        }

        if (novoStatus == StatusSolicitacao.Cancelada)
        {
            Cancelar();
            return;
        }

        if (novoStatus != (StatusSolicitacao)((int)Status + 1))
        {
            throw new InvalidOperationException(
                "Transição de status inválida.");
        }

        RegistrarAlteracaoStatus(novoStatus);
    }

    public void Cancelar()
    {
        if (Status == StatusSolicitacao.EmExecucao ||
            Status == StatusSolicitacao.Concluida ||
            Status == StatusSolicitacao.Cancelada)
        {
            throw new InvalidOperationException(
                "Não é possível cancelar esta solicitação.");
        }

        RegistrarAlteracaoStatus(StatusSolicitacao.Cancelada);
    }

    private void RegistrarAlteracaoStatus(
        StatusSolicitacao novoStatus)
    {
        var statusAnterior = Status;

        Status = novoStatus;

        Historico.Add(new HistoricoSolicitacao(
            this,
            statusAnterior,
            novoStatus));
    }

    private static string GerarNumero()
    {
        var sufixo = Guid.NewGuid()
            .ToString("N")[..8]
            .ToUpperInvariant();

        return $"SOL-{DateTime.UtcNow:yyyyMMdd}-{sufixo}";
    }
}
