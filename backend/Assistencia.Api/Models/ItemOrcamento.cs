using Assistencia.Api.Enums;

namespace Assistencia.Api.Models;

public class ItemOrcamento
{
    public const int TamanhoMaximoDescricao = 500;

    public int Id { get; private set; }

    public int OrcamentoId { get; private set; }
    public Orcamento? Orcamento { get; private set; }

    public string Descricao { get; private set; }
        = string.Empty;

    public TipoItemOrcamento Tipo { get; private set; }

    public int Quantidade { get; private set; }

    public decimal ValorUnitario { get; private set; }

    public decimal ValorTotal =>
        Quantidade * ValorUnitario;

    // Utilizado pelo Entity Framework.
    private ItemOrcamento()
    {
    }

    public ItemOrcamento(
        string descricao,
        TipoItemOrcamento tipo,
        int quantidade,
        decimal valorUnitario)
    {
        ValidarDescricao(descricao);
        ValidarTipo(tipo);
        ValidarQuantidade(quantidade);
        ValidarValorUnitario(valorUnitario);

        Descricao = descricao.Trim();
        Tipo = tipo;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
    }

    public void Alterar(
        string descricao,
        TipoItemOrcamento tipo,
        int quantidade,
        decimal valorUnitario)
    {
        ValidarDescricao(descricao);
        ValidarTipo(tipo);
        ValidarQuantidade(quantidade);
        ValidarValorUnitario(valorUnitario);

        Descricao = descricao.Trim();
        Tipo = tipo;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
    }

    private static void ValidarDescricao(
        string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException(
                "A descrição do item é obrigatória.");
        }

        if (descricao.Trim().Length >
            TamanhoMaximoDescricao)
        {
            throw new ArgumentException(
                $"A descrição do item deve possuir no máximo " +
                $"{TamanhoMaximoDescricao} caracteres.");
        }
    }

    private static void ValidarTipo(
        TipoItemOrcamento tipo)
    {
        if (!Enum.IsDefined(tipo))
        {
            throw new ArgumentException(
                "O tipo do item é inválido.");
        }
    }

    private static void ValidarQuantidade(
        int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new ArgumentException(
                "A quantidade deve ser maior que zero.");
        }
    }

    private static void ValidarValorUnitario(
        decimal valorUnitario)
    {
        if (valorUnitario < 0)
        {
            throw new ArgumentException(
                "O valor unitário não pode ser negativo.");
        }
    }
}