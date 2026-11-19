using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class ExecucaoServico
{
    public const int TamanhoMaximoObservacao = 1000;

    private readonly List<HistoricoExecucaoServico> _historico = new();

    public int Id { get; private set; }
    public int OrcamentoId { get; private set; }
    public Orcamento? Orcamento { get; private set; }
    public StatusExecucaoServico Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataInicio { get; private set; }
    public DateTime? DataConclusao { get; private set; }
    public string? ObservacaoInicio { get; private set; }
    public string? ObservacaoFinal { get; private set; }

    public IReadOnlyCollection<HistoricoExecucaoServico> Historico =>
        _historico.AsReadOnly();

    private ExecucaoServico()
    {
    }

    public ExecucaoServico(Orcamento orcamento)
    {
        ArgumentNullException.ThrowIfNull(orcamento);

        if (orcamento.Status != StatusOrcamento.Aprovado)
        {
            throw new InvalidOperationException(
                "A execução somente pode ser criada para " +
                "um orçamento aprovado.");
        }

        Orcamento = orcamento;
        OrcamentoId = orcamento.Id;
        Status = StatusExecucaoServico.AguardandoInicio;
        DataCriacao = DateTime.UtcNow;

        RegistrarHistorico(
            "Execução criada. Aguardando o início do serviço.");
    }

    public void Iniciar(string? observacao = null)
    {
        if (Status != StatusExecucaoServico.AguardandoInicio)
        {
            throw new InvalidOperationException(
                "Somente uma execução aguardando início pode ser iniciada.");
        }

        ObservacaoInicio = ValidarObservacao(observacao);
        Status = StatusExecucaoServico.EmAndamento;
        DataInicio = DateTime.UtcNow;

        RegistrarHistorico(
            string.IsNullOrWhiteSpace(ObservacaoInicio)
                ? "Execução do serviço iniciada."
                : $"Execução iniciada: {ObservacaoInicio}");
    }

    public void Concluir(string? observacaoFinal = null)
    {
        if (Status != StatusExecucaoServico.EmAndamento)
        {
            throw new InvalidOperationException(
                "Somente uma execução em andamento pode ser concluída.");
        }

        ObservacaoFinal = ValidarObservacao(observacaoFinal);
        Status = StatusExecucaoServico.Concluida;
        DataConclusao = DateTime.UtcNow;

        RegistrarHistorico(
            string.IsNullOrWhiteSpace(ObservacaoFinal)
                ? "Execução do serviço concluída."
                : $"Execução concluída: {ObservacaoFinal}");
    }

    private static string? ValidarObservacao(string? observacao)
    {
        if (string.IsNullOrWhiteSpace(observacao))
        {
            return null;
        }

        var texto = observacao.Trim();

        if (texto.Length > TamanhoMaximoObservacao)
        {
            throw new ArgumentException(
                $"A observação deve possuir no máximo " +
                $"{TamanhoMaximoObservacao} caracteres.");
        }

        return texto;
    }

    private void RegistrarHistorico(string observacao)
    {
        _historico.Add(
            new HistoricoExecucaoServico(
                Status,
                observacao));
    }
}
