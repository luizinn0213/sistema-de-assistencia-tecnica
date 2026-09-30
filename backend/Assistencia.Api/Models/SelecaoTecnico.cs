using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class SelecaoTecnico
{
    public int Id { get; private set; }
    public int SolicitacaoId { get; private set; }

    public int TecnicoId { get; private set; }
    public Tecnico? Tecnico { get; private set; }

    public int EspecialidadeId { get; private set; }
    public Especialidade? Especialidade { get; private set; }

    public StatusSelecaoTecnico Status { get; private set; }

    public DateTime DataSelecao { get; private set; }
    public DateTime? DataResposta { get; private set; }

    public ICollection<HistoricoSelecaoTecnico> Historico
        { get; private set; } = new List<HistoricoSelecaoTecnico>();

    // Utilizado pelo Entity Framework
    public SelecaoTecnico()
    {
    }

    public SelecaoTecnico(
        int solicitacaoId,
        Tecnico tecnico,
        Especialidade especialidade)
    {
        if (solicitacaoId <= 0)
        {
            throw new ArgumentException(
                "A solicitação é obrigatória.");
        }

        ArgumentNullException.ThrowIfNull(tecnico);
        ArgumentNullException.ThrowIfNull(especialidade);

        if (!tecnico.Ativo)
        {
            throw new InvalidOperationException(
                "O técnico está inativo.");
        }

        if (!tecnico.Disponivel)
        {
            throw new InvalidOperationException(
                "O técnico não está disponível.");
        }

        if (!especialidade.Ativa)
        {
            throw new InvalidOperationException(
                "A especialidade está inativa.");
        }

        SolicitacaoId = solicitacaoId;

        Tecnico = tecnico;
        TecnicoId = tecnico.Id;

        Especialidade = especialidade;
        EspecialidadeId = especialidade.Id;

        Status = StatusSelecaoTecnico.AguardandoResposta;
        DataSelecao = DateTime.UtcNow;

        Historico.Add(new HistoricoSelecaoTecnico(
            StatusSelecaoTecnico.AguardandoResposta,
            "Técnico selecionado. Aguardando resposta."));
    }

    public void Aceitar()
    {
        ValidarAguardandoResposta();

        Status = StatusSelecaoTecnico.Aceita;
        DataResposta = DateTime.UtcNow;

        Historico.Add(new HistoricoSelecaoTecnico(
            Status,
            "Solicitação aceita pelo técnico."));
    }

    public void Recusar(string? motivo)
    {
        ValidarAguardandoResposta();

        Status = StatusSelecaoTecnico.Recusada;
        DataResposta = DateTime.UtcNow;

        var observacao = string.IsNullOrWhiteSpace(motivo)
            ? "Solicitação recusada pelo técnico."
            : $"Solicitação recusada: {motivo.Trim()}";

        Historico.Add(new HistoricoSelecaoTecnico(
            Status,
            observacao));
    }

    private void ValidarAguardandoResposta()
    {
        if (Status != StatusSelecaoTecnico.AguardandoResposta)
        {
            throw new InvalidOperationException(
                "Essa seleção já recebeu uma resposta.");
        }
    }
}