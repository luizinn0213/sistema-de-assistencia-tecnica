using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class Orcamento
{
    private readonly List<ItemOrcamento> _itens = new();

    private readonly List<HistoricoOrcamento>
        _historico = new();

    public int Id { get; private set; }

    public int DiagnosticoId { get; private set; }
    public Diagnostico? Diagnostico { get; private set; }

    public StatusOrcamento Status { get; private set; }

    public DateTime DataCriacao { get; private set; }
    public DateTime? DataEnvio { get; private set; }
    public DateTime? DataResposta { get; private set; }

    public IReadOnlyCollection<ItemOrcamento> Itens =>
        _itens.AsReadOnly();

    public IReadOnlyCollection<HistoricoOrcamento> Historico =>
        _historico.AsReadOnly();

    public decimal ValorTotal =>
        _itens.Sum(item => item.ValorTotal);

    // Utilizado pelo Entity Framework.
    private Orcamento()
    {
    }

    public Orcamento(Diagnostico diagnostico)
    {
        ArgumentNullException.ThrowIfNull(diagnostico);

        if (diagnostico.Status !=
            StatusDiagnostico.Concluido)
        {
            throw new InvalidOperationException(
                "O orçamento somente pode ser criado para " +
                "um diagnóstico concluído.");
        }

        Diagnostico = diagnostico;
        DiagnosticoId = diagnostico.Id;

        Status = StatusOrcamento.EmElaboracao;
        DataCriacao = DateTime.UtcNow;

        RegistrarHistorico(
            "Orçamento criado.");
    }

    public ItemOrcamento AdicionarItem(
        string descricao,
        TipoItemOrcamento tipo,
        int quantidade,
        decimal valorUnitario)
    {
        ValidarEmElaboracao();

        var item = new ItemOrcamento(
            descricao,
            tipo,
            quantidade,
            valorUnitario);

        _itens.Add(item);

        return item;
    }

    public void AlterarItem(
        ItemOrcamento item,
        string descricao,
        TipoItemOrcamento tipo,
        int quantidade,
        decimal valorUnitario)
    {
        ValidarEmElaboracao();
        ValidarItemPertencente(item);

        item.Alterar(
            descricao,
            tipo,
            quantidade,
            valorUnitario);
    }

    public void RemoverItem(ItemOrcamento item)
    {
        ValidarEmElaboracao();
        ValidarItemPertencente(item);

        _itens.Remove(item);
    }

    public void Enviar()
    {
        ValidarEmElaboracao();

        if (_itens.Count == 0)
        {
            throw new InvalidOperationException(
                "O orçamento precisa possuir pelo menos " +
                "um item antes do envio.");
        }

        Status = StatusOrcamento.Enviado;
        DataEnvio = DateTime.UtcNow;

        RegistrarHistorico(
            "Orçamento enviado ao cliente.");
    }

    public void Aprovar(string? observacao = null)
    {
        ValidarEnviado();

        Status = StatusOrcamento.Aprovado;
        DataResposta = DateTime.UtcNow;

        RegistrarHistorico(
            string.IsNullOrWhiteSpace(observacao)
                ? "Orçamento aprovado pelo cliente."
                : $"Orçamento aprovado: {observacao.Trim()}");
    }

    public void Rejeitar(string? motivo)
    {
        ValidarEnviado();

        Status = StatusOrcamento.Rejeitado;
        DataResposta = DateTime.UtcNow;

        RegistrarHistorico(
            string.IsNullOrWhiteSpace(motivo)
                ? "Orçamento rejeitado pelo cliente."
                : $"Orçamento rejeitado: {motivo.Trim()}");
    }

    public void Cancelar(string? motivo)
    {
        if (Status != StatusOrcamento.EmElaboracao &&
            Status != StatusOrcamento.Enviado)
        {
            throw new InvalidOperationException(
                "O orçamento não pode ser cancelado " +
                "no estado atual.");
        }

        Status = StatusOrcamento.Cancelado;
        DataResposta = DateTime.UtcNow;

        RegistrarHistorico(
            string.IsNullOrWhiteSpace(motivo)
                ? "Orçamento cancelado."
                : $"Orçamento cancelado: {motivo.Trim()}");
    }

    private void ValidarEmElaboracao()
    {
        if (Status != StatusOrcamento.EmElaboracao)
        {
            throw new InvalidOperationException(
                "Os itens somente podem ser alterados enquanto " +
                "o orçamento está em elaboração.");
        }
    }

    private void ValidarEnviado()
    {
        if (Status != StatusOrcamento.Enviado)
        {
            throw new InvalidOperationException(
                "Somente um orçamento enviado pode receber " +
                "uma resposta.");
        }
    }

    private void ValidarItemPertencente(
        ItemOrcamento item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!_itens.Contains(item))
        {
            throw new InvalidOperationException(
                "O item não pertence a este orçamento.");
        }
    }

    private void RegistrarHistorico(
        string observacao)
    {
        _historico.Add(
            new HistoricoOrcamento(
                Status,
                observacao));
    }
}